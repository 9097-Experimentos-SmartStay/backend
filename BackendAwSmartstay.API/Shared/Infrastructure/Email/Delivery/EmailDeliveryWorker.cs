using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Delivery;

/// <summary>
///     Background worker that delivers the queued e-mails through the configured <see cref="IEmailTransport"/>,
///     retrying transient failures a few times with a growing delay.
/// </summary>
/// <remarks>
///     Delivery is at-most-once: a message still in the queue when the process stops is lost. Every flow that
///     sends an e-mail lets the user ask for it again (resend verification, request a new reset link), which is
///     why a persistent outbox was not needed.
/// </remarks>
public class EmailDeliveryWorker(
    EmailDeliveryQueue queue,
    IEmailTransport transport,
    ILogger<EmailDeliveryWorker> logger) : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in queue.ReadAllAsync(stoppingToken))
                await DeliverWithRetriesAsync(message, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
    }

    private async Task DeliverWithRetriesAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await transport.DeliverAsync(message, stoppingToken);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                if (attempt >= RetryDelays.Length)
                {
                    logger.LogError(exception, "E-mail '{Subject}' to {Recipient} could not be delivered; giving up.",
                        message.Subject, message.To);
                    return;
                }

                logger.LogWarning(exception, "E-mail '{Subject}' to {Recipient} failed (attempt {Attempt}); retrying.",
                    message.Subject, message.To, attempt + 1);
                await Task.Delay(RetryDelays[attempt], stoppingToken);
            }
        }
    }
}
