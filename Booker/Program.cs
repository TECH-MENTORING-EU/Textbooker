using Booker.Areas.Identity.Utilities;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Serilog;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

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
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logsPath, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();


builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages().AddViewOptions(options =>
{
    options.HtmlHelperOptions.FormInputRenderMode = Microsoft.AspNetCore.Mvc.Rendering.FormInputRenderMode.AlwaysUseCurrentCulture;
});

// Add booker services to the container
builder.Services.AddBookerServices(configuration);
builder.Services.AddRateLimitPolicies();

builder.Services.AddDbContext<DataContext>(options =>
{
    //options.UseInMemoryDatabase("InMemoryDatabaseName");
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DataContext>()
        .AddErrorDescriber<ErrorDescriber>();

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

app.UseAuthentication();
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

app.Run();