using Booker.Areas.Identity.Utilities;
using Booker.Data;
using Booker.Services;
using Booker.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Serilog;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using System.Security.Claims;
using Serilog.Events;

ResourceManagerHack.OverrideComponentModelAnnotationsResourceManager();

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddUserSecrets<Program>() // Replace `Program` with your project's main class
    .AddEnvironmentVariables().Build();

if (await StartupUtilities.RunMaintenanceMode(configuration, args))
{
    return;
}


// Register IMemoryCache in DI container
builder.Services.AddMemoryCache();

// Configure Serilog
var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logsPath, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();


builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages()
    .AddViewOptions(options =>
{
    options.HtmlHelperOptions.FormInputRenderMode = Microsoft.AspNetCore.Mvc.Rendering.FormInputRenderMode.AlwaysUseCurrentCulture;
})
    .AddCustomRoutes()
    .AddAuthorizationPolicies();

// Add booker services to the container
builder.Services.AddBookerServices(configuration);
builder.Services.AddRateLimitPolicies();

builder.Services.AddDbContext<DataContext>(options => options.ConfigureDatabase(configuration));

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<DataContext>()
        .AddErrorDescriber<ErrorDescriber>();

builder.Services.ConfigureAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

var cultureInfo = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    // Enable serving of SCSS files to make sourcemap work

    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".scss"] = "text/x-scss";
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider
    });
}
else
{
    app.UseStaticFiles();
}




app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Status/{0}");

app.UseAuthentication();
app.Use(async (context, next) =>
{
    using var scope = app.Services.CreateScope();
    var sessionCacheManager = scope.ServiceProvider.GetRequiredService<SessionCacheManager>();
    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();
    if (!await sessionCacheManager.CheckSession(context))
    {
        await signInManager.SignOutAsync();
        context.User = new ClaimsPrincipal();
    }
    await next();
});
app.UseAuthorization();
app.UseRateLimiter();

app.MapRazorPages();
if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
}
else
{
    await app.MigrateDatabaseAsync(configuration);
}

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
    await app.InitializeDatabaseAsync();
}

await app.InitializeRolesAsync();

if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    const string testUserName = "accessibility-user";
    var testUser = await userManager.FindByNameAsync(testUserName);
    if (testUser == null)
    {
        testUser = new User
        {
            UserName = testUserName,
            Email = "accessibility@example.test",
            EmailConfirmed = true
        };
        var password = configuration["AccessibilityTests:Password"]
            ?? throw new InvalidOperationException("Accessibility test password is not configured.");
        var result = await userManager.CreateAsync(testUser, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
    if (!await userManager.IsInRoleAsync(testUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(testUser, "Admin");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }
    }

    if (!await dbContext.Items.AnyAsync())
    {
        var book = await dbContext.Books.SingleAsync(candidate => candidate.Title == "Ponad słowami 1 cz. 1");
        dbContext.Items.Add(new Item
        {
            BookId = book.Id,
            Book = book,
            UserId = testUser.Id,
            User = testUser,
            Price = 29.90m,
            CreatedAt = new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc),
            Description = "Testowe ogłoszenie używane do weryfikacji dostępności.",
            State = "Bardzo dobry",
            Photo = "/img/default-book.svg"
        });
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
