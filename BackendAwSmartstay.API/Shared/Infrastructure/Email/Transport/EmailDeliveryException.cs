namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>
///     The mail system refused a message. <see cref="IsPermanent"/> tells the outbox whether a retry can succeed.
///     The message is a short, secret-free description (it is stored as the last error of the outbox e-mail).
/// </summary>
public sealed class EmailDeliveryException(string message, bool isPermanent, Exception? innerException = null)
    : Exception(message, innerException)
{
    public bool IsPermanent { get; } = isPermanent;
}
