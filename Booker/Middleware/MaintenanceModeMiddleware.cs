using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Booker.Middleware;

public sealed class MaintenanceModeMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment environment)
{
    private readonly string maintenancePagePath = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "_MaintenancePage.html");

    public async Task InvokeAsync(HttpContext context)
    {
        var isMaintenanceEnabled = configuration.GetValue<bool>("Maintenance");

        if (!isMaintenanceEnabled)
        {
            await next(context);
            return;
        }

        if (!File.Exists(maintenancePagePath))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Maintenance page not found.");
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(maintenancePagePath);
    }
}
