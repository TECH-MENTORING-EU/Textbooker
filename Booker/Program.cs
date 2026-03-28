using Booker.Areas.Identity.Utilities;
using Booker.Data;
using Booker.Middleware;
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

var isMaintenanceEnabled = configuration.GetValue<bool>("Maintenance");


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

builder.Services.AddDbContext<DataContext>(options =>
{
    //options.UseInMemoryDatabase("InMemoryDatabaseName");
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseCompatibilityLevel(110));
});

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

if (configuration.GetValue<bool>("MaintenanceMode"))
{
    app.Run(async (context) =>
    {
        context.Response.StatusCode = 503;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync("""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>TextBooker – Wracamy we wrześniu!</title>
                <link rel="preconnect" href="https://fonts.googleapis.com">
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                <link href="https://fonts.googleapis.com/css2?family=Nunito+Sans:ital,opsz,wght@0,6..12,200..1000;1,6..12,200..1000&display=swap" rel="stylesheet">
                <link rel="icon" type="image/png" href="/favicons/favicon-96x96.png" sizes="96x96" />
                <link rel="stylesheet" href="/css/site.css" />
            </head>
            <body>
                <main class="container">
                    <section class="maintenance">
                        <h1>🎒 Wracamy we wrześniu wraz z obsługą kilku szkół</h1>
                        <p class="subtitle">
                            Tymczasem zapraszamy na webinar <strong>On .NET Live</strong>, w którym wystąpili członkowie naszego zespołu,
                            opowiadając o programie nauki, wykorzystywaniu AI w projekcie oraz web developmencie aplikacji
                            z użyciem technologii <strong>Razor Pages</strong> i <strong>HTMX</strong>.
                        </p>
                        <div class="video-wrapper">
                            <iframe
                                src="https://www.youtube.com/embed/OEfsOWjlPF0"
                                title="On .NET Live – TextBooker"
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                                allowfullscreen>
                            </iframe>
                        </div>
                        <div class="social-links">
                            <a href="https://www.facebook.com/profile.php?id=61578317232992" target="_blank">Facebook</a>
                            <a href="https://www.instagram.com/textbooker.pl/" target="_blank">Instagram</a>
                            <a href="mailto:support@textbooker.pl">support@textbooker.pl</a>
                        </div>
                    </section>
                </main>
            </body>
            </html>
            """);
    });

    Log.Information("Maintenance mode is enabled. Skipping database migrations and initialization.");
    app.Run();
    return;
}

app.UseMiddleware<MaintenanceModeMiddleware>();

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

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
}

await app.MigrateDatabaseAsync(configuration);

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

await app.InitializeRolesAsync();

app.Run();