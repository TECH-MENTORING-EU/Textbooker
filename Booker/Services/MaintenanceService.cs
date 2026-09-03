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

            await AutoCloseStaleReservations(scope);
        }
    }

    /// <summary>
    /// Reservations with no seller decision for ~30 days count as sold so the
    /// listing does not linger as reserved and both parties can rate.
    /// </summary>
    private static async Task AutoCloseStaleReservations(IServiceScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MaintenanceService>>();
        var itemManager = scope.ServiceProvider.GetRequiredService<ItemManager>();

        try
        {
            var closed = await itemManager.AutoCloseStaleReservationsAsync();
            if (closed > 0)
            {
                logger.LogInformation("Auto-closed {Count} stale reservations as sold.", closed);
            }
        }
        catch (Exception ex)
        {
            // Never let the maintenance loop crash: a failed auto-close retries on the next tick.
            logger.LogError(ex, "Auto-close of stale reservations failed; will retry next tick.");
        }
    }
}
