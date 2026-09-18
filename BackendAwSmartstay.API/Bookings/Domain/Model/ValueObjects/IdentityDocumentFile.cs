using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     The photo or scan of the identity document uploaded at the check-in (US-08 scenario 2): JPEG, PNG or PDF of at
///     most 5 MB. The type is decided by the file's content (its signature), never by its name or the declared type.
/// </summary>
public sealed record IdentityDocumentFile
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private IdentityDocumentFile(string contentType, long sizeBytes)
    {
        ContentType = contentType;
        SizeBytes = sizeBytes;
    }

    public string ContentType { get; }
    public long SizeBytes { get; }

    public static IdentityDocumentFile Inspect(ReadOnlySpan<byte> content)
    {
        if (content.Length == 0)
            throw new InvalidFieldException("document", BookingErrorCodes.CheckInDocumentRequired, "Upload a photo or scan of your identity document.");
        if (content.Length > MaxSizeBytes)
            throw new InvalidFieldException("document", BookingErrorCodes.CheckInDocumentTooLarge, "The document file cannot exceed 5 MB.");

        var contentType = content switch
        {
            [0xFF, 0xD8, 0xFF, ..] => "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => "image/png",
            [0x25, 0x50, 0x44, 0x46, 0x2D, ..] => "application/pdf",
            _ => null
        };
        if (contentType is null)
            throw new InvalidFieldException("document", BookingErrorCodes.CheckInDocumentFileType, "The document must be a JPG, PNG or PDF file.");

        return new IdentityDocumentFile(contentType, content.Length);
    }
}
