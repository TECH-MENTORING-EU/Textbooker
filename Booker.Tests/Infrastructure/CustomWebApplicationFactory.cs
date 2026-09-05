using Amazon.S3;
using Booker.Data;
using Booker.TestUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Booker.Tests.Infrastructure;

/// <summary>
/// Boots the real app in the "Testing" environment on a shared SQLite in-memory database,
/// with a recording email sender and a fake S3 adapter. One factory per test class:
/// the app's IMemoryCache (StaticDataManager lists, session state) is per-host, so a
/// shared factory would leak cached data between classes.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public S3Recorder S3 { get; } = new();
    public FakeEmailSender Emails { get; } = new();

    public CustomWebApplicationFactory()
    {
        // Keep the connection open for the host's lifetime - the in-memory database
        // persists only as long as the connection does.
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Swap the SqlServer DbContext for SQLite on the shared connection.
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.AddDbContext<DataContext>(options => options.UseSqlite(_connection));

            // Never send mail from tests.
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);

            // Never touch real R2/S3 from tests.
            services.RemoveAll<Lazy<IAmazonS3>>();
            services.AddSingleton(new Lazy<IAmazonS3>(S3.BuildClient));
        });
    }

    protected override void Dispose(bool disposing)
    {
        _connection.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>A client that surfaces 302s instead of following them (auth-redirect assertions).</summary>
    public HttpClient CreateNoRedirectClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });
}
