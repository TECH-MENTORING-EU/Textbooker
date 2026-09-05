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

            await ReleaseStaleReservations(scope);
        }
    }

    /// <summary>
    /// Reservations the seller never decided on are released after
    /// <see cref="ItemManager.AutoCloseDays"/>, so listings do not linger as
    /// reserved forever.
    /// </summary>
    private static async Task ReleaseStaleReservations(IServiceScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MaintenanceService>>();
        var itemManager = scope.ServiceProvider.GetRequiredService<ItemManager>();

        try
        {
            var released = await itemManager.AutoCloseStaleReservationsAsync();
            if (released > 0)
            {
                logger.LogInformation("Auto-close released {Count} stale reservations.", released);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auto-close of stale reservations failed; retrying on the next tick.");
        }
    }
}
