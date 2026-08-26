using Amazon.S3;
using Booker.Data;
using Booker.TestUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Booker.E2e;

/// <summary>
/// Boots the real app in the "Testing" environment on a real Kestrel socket
/// (port 0) instead of the in-memory TestServer, so a real Chromium can drive
/// it. Fake adapters come from the shared Booker.TestUtils project: SQLite
/// in-memory, recording S3, recording e-mail sender.
/// </summary>
/// <remarks>
/// The factory machinery builds the app twice (dummy TestServer host for WAF +
/// the live Kestrel host), so both boot paths run over the database. To keep
/// that safe: every host gets its own connection into one shared-cache
/// in-memory database (a single SqliteConnection instance is not thread-safe),
/// and the schema plus the Admin role are created up front so both boots only
/// ever read existing state.
/// </remarks>
public sealed class E2eWebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionString = "DataSource=file:e2e?mode=memory&cache=shared";

    // Keeps the shared-cache in-memory database alive for the whole fixture.
    private readonly SqliteConnection _anchor = new(ConnectionString);

    // The live Kestrel host; WAF only knows the dummy TestServer host, so the
    // fixture itself must stop this one on teardown.
    private IHost? _appHost;

    public string BaseUrl { get; private set; } = "";
    public int SeededItemId { get; private set; }
    public S3Recorder S3 { get; } = new();
    public FakeEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseUrls("http://127.0.0.1:0");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.AddDbContext<DataContext>(options => options.UseSqlite(ConnectionString));
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<Lazy<IAmazonS3>>();
            services.AddSingleton(new Lazy<IAmazonS3>(S3.BuildClient));
        });
    }

    // WebApplicationFactory expects CreateHost to return a started host it can
    // probe through TestServer; we build that dummy host, then separately build
    // and start the real Kestrel host and read back the bound address.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();
        builder.ConfigureWebHost(web => web.UseKestrel());
        var host = builder.Build();
        host.Start();
        _appHost = host;
        BaseUrl = host.Services
            .GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
            .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!
            .Addresses.First();
        return dummyHost;
    }

    public async Task InitializeAsync()
    {
        _anchor.Open();
        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(_anchor).Options;
        await using (var context = new DataContext(options))
        {
            // Both app boots must only ever observe existing state: the schema
            // (EnsureCreated is a no-op then) and the Admin role (RoleExists
            // short-circuits InitializeRolesAsync's insert).
            await context.Database.EnsureCreatedAsync();
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.Add(new IdentityRole<int>("Admin") { NormalizedName = "ADMIN" });
                await context.SaveChangesAsync();
            }
        }

        CreateClient(); // forces host creation; populates BaseUrl. No requests - all
                        // E2E traffic goes to the Kestrel host only.

        var school = await TestSeed.CreateSchoolAsync(Services, "E2E School", "e2e.edu.pl");
        await TestSeed.CreateUserAsync(Services, "e2euser", "e2euser@e2e.edu.pl", school);
        await TestSeed.CreateUserAsync(Services, "e2eother", "e2eother@e2e.edu.pl", school);
        // Throwaway account for wrong-password attempts (lockout burns 5/5 min).
        await TestSeed.CreateUserAsync(Services, "e2evictim", "e2evictim@e2e.edu.pl", school);
        SeededItemId = await TestSeed.CreateItemAsync(
            Services, ownerId: await FindUserIdAsync("e2eother"), price: 12.50m);
    }

    private async Task<int> FindUserIdAsync(string name)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByNameAsync(name);
        return user!.Id;
    }

    // Explicit interface implementation: WAF's own public DisposeAsync would
    // otherwise be hidden (and skipped) by a Task-returning member.
    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_appHost is not null)
        {
            await _appHost.StopAsync();
            _appHost.Dispose();
        }
        await base.DisposeAsync(); // tears the dummy TestServer host down
        _anchor.Dispose();
        await Browsers.Shared.DisposeAsync(); // the browser outlives only this collection
    }
}
