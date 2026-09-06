using System;

namespace Booker.Services;

public class MaintenanceService(IServiceProvider services) : BackgroundService
{


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = services.CreateScope();
            var sessionCacheManager = scope.ServiceProvider.GetRequiredService<SessionCacheManager>();

            await sessionCacheManager.WritebackSessions();
            sessionCacheManager.CleanupSessions();            
        }
    }
}
