namespace Booker.Services;

/// <summary>
/// Drains <see cref="WelcomeEmailQueue"/> and performs the actual SMTP send with retry,
/// off the activation request path. A new DI scope is created per email so SendMailSvc
/// (and any future dependency it gains) is resolved correctly outside the request scope.
/// </summary>
public class WelcomeEmailQueueService(
    WelcomeEmailQueue queue,
    IServiceProvider serviceProvider,
    ILogger<WelcomeEmailQueueService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = serviceProvider.CreateScope();
            var mailSvc = scope.ServiceProvider.GetRequiredService<SendMailSvc>();
            await WelcomeEmailSender.SendWithRetryAsync(mailSvc, logger, email);
        }
    }
}
