using Cortex.Mediator.Commands;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
/// Behavior for logging commands in the mediator pipeline.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
public class LoggingCommandBehavior<TCommand>
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles the command with logging.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(
        TCommand command,
        CommandHandlerDelegate next,
        CancellationToken ct)
    {
        // Log before/after
        await next();
    }
}
