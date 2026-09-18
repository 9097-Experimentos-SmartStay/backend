using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;

/// <summary>
///     Background loop of the outbox: every <c>Email:Outbox:PollIntervalSeconds</c> it delivers the due e-mails
///     (<see cref="IEmailDispatcher"/>, in its own scope). The first round runs at startup, so e-mails left pending
///     by a restart or a redeploy go out as soon as the application is back.
/// </summary>
public class OutboxEmailDispatchWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<EmailSettings> options,
    ILogger<OutboxEmailDispatchWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(options.Value.Outbox.PollIntervalSeconds);
        using var timer = new PeriodicTimer(interval);
        try
        {
            do
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<IEmailDispatcher>().DispatchDueAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // e.g. the database is unreachable: nothing is lost, the next round tries again.
                    logger.LogError(exception, "E-mail outbox round failed; retrying in {Interval}.", interval);
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
    }
}
