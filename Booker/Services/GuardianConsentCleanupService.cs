using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace Booker.Services;

/// <summary>
/// RODO - Phase 4: Automatically deletes unconfirmed minor accounts whose guardian consent
/// has expired (>7 days). Runs periodically as a background service.
/// </summary>
public class GuardianConsentCleanupService(IServiceProvider serviceProvider, ILogger<GuardianConsentCleanupService> logger)
    : BackgroundService
{
    private const int DefaultCleanupDelayMinutes = 5; // Initial delay before first cleanup
    private PeriodicTimer? _timer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = serviceProvider.GetService<IOptions<GuardianConsentOptions>>()?.Value;
        var intervalMinutes = options?.CleanupIntervalMinutes > 0 ? options.CleanupIntervalMinutes : DefaultCleanupDelayMinutes;
        var retentionDays = options?.ConfirmedDataRetentionDays > 0 ? options.ConfirmedDataRetentionDays : 90;

        // Wait before first run
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        
        logger.LogInformation("GuardianConsentCleanupService started. First cleanup in {DelayMinutes} minutes.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupExpiredConsentsAsync(stoppingToken, retentionDays);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during guardian consent cleanup.");
                // Continue running despite errors
            }
        }
    }

    private async Task CleanupExpiredConsentsAsync(CancellationToken cancellationToken, int retentionDays)
    {
        // Find all unconfirmed, expired consents
        List<int> expiredUserIds;
        using (var queryScope = serviceProvider.CreateScope())
        {
            var queryContext = queryScope.ServiceProvider.GetRequiredService<DataContext>();
            expiredUserIds = await queryContext.GuardianConsents
                .Where(gc => gc.ConfirmedAtUtc == null && gc.ExpiresAtUtc <= DateTime.UtcNow)
                .Select(gc => gc.UserId)
                .ToListAsync(cancellationToken);
        }

        if (expiredUserIds.Count == 0)
        {
            logger.LogDebug("No expired guardian consents found.");
            await AnonymizeConfirmedConsentsAsync(retentionDays, cancellationToken);
            return;
        }

        logger.LogInformation("Found {Count} expired guardian consent(s). Starting cleanup.", expiredUserIds.Count);
        var deletedCount = 0;
        var failedCount = 0;

        foreach (var userId in expiredUserIds)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var userPhotoManager = scope.ServiceProvider.GetRequiredService<UserPhotoManager>();

            try
            {
                await using var transaction = await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var expiredConsent = await context.GuardianConsents
                    .SingleOrDefaultAsync(
                        gc => gc.UserId == userId
                            && gc.ConfirmedAtUtc == null
                            && gc.ExpiresAtUtc <= DateTime.UtcNow,
                        cancellationToken);
                if (expiredConsent is null)
                {
                    continue;
                }

                // Get user and collect photo keys before deletion
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User ID {UserId} not found but has expired consent record. Removing consent.", userId);
                    context.GuardianConsents.Remove(expiredConsent);
                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    deletedCount++;
                    continue;
                }

                // Collect photo keys before deletion
                var photoKeys = await userPhotoManager.CollectPhotoKeysAsync(user);

                // Delete the user (cascades to GuardianConsent and related data)
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to delete user ID {UserId}: {Errors}", userId,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    await transaction.RollbackAsync(cancellationToken);
                    failedCount++;
                    continue;
                }

                await transaction.CommitAsync(cancellationToken);

                // Delete photos from storage (after successful DB deletion)
                await userPhotoManager.DeleteFromStorageAsync(user.Id, photoKeys);
                deletedCount++;

                logger.LogInformation("Deleted unconfirmed user ID {UserId} due to expired guardian consent.", userId);
            }
            catch (Exception ex)
            {
                failedCount++;
                logger.LogError(ex, "Error deleting user ID {UserId} with expired consent.", userId);
                // Continue to next user
            }
        }

        logger.LogInformation(
            "Guardian consent cleanup completed. Deleted {DeletedCount} account(s); {FailedCount} failed.",
            deletedCount,
            failedCount);

        await AnonymizeConfirmedConsentsAsync(retentionDays, cancellationToken);
    }

    private async Task AnonymizeConfirmedConsentsAsync(int retentionDays, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        var consents = await context.GuardianConsents
            .Where(gc => gc.ConfirmedAtUtc != null
                && gc.ConfirmedAtUtc <= cutoff
                && (gc.GuardianEmail != null || gc.ConfirmationIpAddress != null))
            .ToListAsync(cancellationToken);

        if (consents.Count == 0)
        {
            return;
        }

        foreach (var consent in consents)
        {
            consent.GuardianEmail = null;
            consent.ConfirmationIpAddress = null;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Anonymized {Count} confirmed guardian consent(s).", consents.Count);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
        logger.LogInformation("GuardianConsentCleanupService stopped.");
    }
}
