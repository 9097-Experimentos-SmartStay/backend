namespace BackendAwSmartstay.API.Bookings.Application.OutboundServices;

/// <summary>
///     Stores the identity documents uploaded at the check-in (US-08). Adapters must keep them confidential (they
///     contain personal data) and join the caller's unit of work when they can, so a failed check-in stores nothing.
/// </summary>
public interface IDocumentStorage
{
    /// <summary>Stores <paramref name="content"/> and returns its key.</summary>
    Task<string> SaveAsync(byte[] content, string contentType, CancellationToken cancellationToken = default);
}
