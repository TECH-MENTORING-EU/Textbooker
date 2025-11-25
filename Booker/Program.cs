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
using Microsoft.AspNetCore.SignalR; // added
using Microsoft.AspNetCore.Components;
using Booker.Messenger; // added

ResourceManagerHack.OverrideComponentModelAnnotationsResourceManager();

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddUserSecrets<Program>() // Replace `Program` with your project's main class
    .AddEnvironmentVariables().Build();

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

// Add Razor Components + interactive server support for islands
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(o => { o.DetailedErrors = builder.Environment.IsDevelopment(); });
// Add SignalR for chat hub
builder.Services.AddSignalR();

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
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
app.UseStaticFiles();
app.Run();