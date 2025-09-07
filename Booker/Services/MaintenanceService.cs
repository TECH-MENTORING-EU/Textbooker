using System;

namespace Booker.Services;

public class MaintenanceService : BackgroundService
{
    public MaintenanceService(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = Services.CreateScope();
            var sessionCacheManager = scope.ServiceProvider.GetRequiredService<SessionCacheManager>();

            await sessionCacheManager.WritebackSessions();
            sessionCacheManager.CleanupSessions();            
        }
    }
}
