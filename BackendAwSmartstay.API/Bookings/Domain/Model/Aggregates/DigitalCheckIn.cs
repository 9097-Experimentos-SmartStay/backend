using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

/// <summary>Result of the automatic document validation.</summary>
public enum CheckInStatus
{
    /// <summary>The document passed the format validation: the check-in was approved.</summary>
    Approved
}

/// <summary>
///     The digital check-in of a booking (US-08): the identity declared by the guest, the reference to the uploaded
///     document (stored encrypted outside the aggregate) and the room access code (stored encrypted). One per booking.
/// </summary>
public class DigitalCheckIn
{
    /// <summary>EF Core constructor.</summary>
    protected DigitalCheckIn()
    {
        DocumentNumber = string.Empty;
        Nationality = string.Empty;
        DocumentFileId = string.Empty;
        DocumentContentType = string.Empty;
        AccessCodeProtected = string.Empty;
    }

    public int Id { get; private set; }
    public int BookingId { get; private set; }
    public int? GuestUserId { get; private set; }
    public IdentityDocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; }
    public string Nationality { get; private set; }

    /// <summary>Key of the uploaded document in the document storage.</summary>
    public string DocumentFileId { get; private set; }
    public string DocumentContentType { get; private set; }
    public long DocumentSizeBytes { get; private set; }

    public CheckInStatus Status { get; private set; }

    /// <summary>The room access code, encrypted by the application.</summary>
    public string AccessCodeProtected { get; private set; }
    public DateTimeOffset AccessCodeValidUntil { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    public GuestIdentityDocument Identity => new(DocumentType, DocumentNumber, Nationality);

    /// <summary>
    ///     Records the approved check-in of <paramref name="booking"/>: the document passed the format validation
    ///     (<see cref="GuestIdentityDocument"/>, <see cref="IdentityDocumentFile"/>) and the stay starts.
    /// </summary>
    public static DigitalCheckIn Approve(Booking booking, GuestIdentityDocument identity, IdentityDocumentFile file,
        string documentFileId, string accessCodeProtected, DateTimeOffset accessCodeValidUntil, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(documentFileId))
            throw new DomainValidationException("The check-in needs the stored document.");
        if (accessCodeValidUntil <= now)
            throw new DomainValidationException("The access code must be valid until the check-out.");

        return new DigitalCheckIn
        {
            BookingId = booking.Id,
            GuestUserId = booking.GuestId?.Value,
            DocumentType = identity.Type,
            DocumentNumber = identity.Number,
            Nationality = identity.Nationality,
            DocumentFileId = documentFileId,
            DocumentContentType = file.ContentType,
            DocumentSizeBytes = file.SizeBytes,
            Status = CheckInStatus.Approved,
            AccessCodeProtected = accessCodeProtected,
            AccessCodeValidUntil = accessCodeValidUntil,
            CompletedAt = now
        };
    }
}
