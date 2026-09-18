namespace BackendAwSmartstay.API.Bookings.Infrastructure.Storage;

/// <summary>A document stored by <see cref="DatabaseDocumentStorage"/> (encrypted content).</summary>
public class StoredDocument
{
    public Guid Id { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public byte[] ProtectedContent { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}
