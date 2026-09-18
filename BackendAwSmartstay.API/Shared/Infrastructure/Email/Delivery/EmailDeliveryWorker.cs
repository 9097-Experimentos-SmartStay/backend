using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Delivery;

/// <summary>
///     Background worker that delivers the queued e-mails through the configured <see cref="IEmailTransport"/>
///     (resolved per message from a new scope: the Brevo transport is a typed <c>HttpClient</c> whose handlers the
///     factory rotates), retrying transient failures a few times with a growing delay. A permanent rejection
///     (<see cref="EmailDeliveryException.IsPermanent"/>) is not retried.
/// </summary>
/// <remarks>
///     Delivery is at-most-once: a message still in the queue when the process stops is lost. Every flow that
///     sends an e-mail lets the user ask for it again (resend verification, request a new reset link).
///     Recipients are masked in the log.
/// </remarks>
public class EmailDeliveryWorker(
    EmailDeliveryQueue queue,
    IServiceScopeFactory scopeFactory,
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
        var recipient = EmailAddressMask.Mask(message.To);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IEmailTransport>().DeliverAsync(message, stoppingToken);
                logger.LogInformation("E-mail '{Subject}' sent to {Recipient}.", message.Subject, recipient);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
            {
                var error = exception is EmailDeliveryException ? exception.Message : $"{exception.GetType().Name}: {exception.Message}";
                if (exception is EmailDeliveryException { IsPermanent: true } || attempt > RetryDelays.Length)
                {
                    logger.LogError("E-mail '{Subject}' to {Recipient} could not be delivered after {Attempts} attempt(s); giving up. {Error}",
                        message.Subject, recipient, attempt, error);
                    return;
                }

                logger.LogWarning("E-mail '{Subject}' to {Recipient} failed (attempt {Attempt}); retrying. {Error}",
                    message.Subject, recipient, attempt, error);
                await Task.Delay(RetryDelays[attempt - 1], stoppingToken);
            }
        }
    }
}
