using Microsoft.Extensions.Hosting.Internal;
using Microsoft.EntityFrameworkCore;
using Booker.Data;
using System.Globalization;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Booker.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Configuration;
using System.Net;
using Booker.Areas.Identity.Utilities;

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
builder.Services.AddRazorPages();

// Add booker services to the container
builder.Services.AddBookerServices(configuration);

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
}

app.Run();