using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Storage;

/// <summary>
///     Document storage in the application database (table <c>stored_documents</c>), encrypted with Data Protection.
/// </summary>
/// <remarks>
///     Chosen for this delivery because Render's free instances have an ephemeral disk (files written locally vanish
///     on every deploy or restart) and the documents are few and small (≤ 5 MB each). The row joins the check-in's
///     unit of work, so a rejected check-in leaves nothing behind, and the content is encrypted at rest because it is
///     an identity document. An object storage adapter (S3, Cloudinary...) can replace it behind
///     <see cref="IDocumentStorage"/> when volume grows.
/// </remarks>
public class DatabaseDocumentStorage(AppDbContext context, ISecretProtector secretProtector, TimeProvider timeProvider)
    : IDocumentStorage
{
    private const string Purpose = "Bookings.IdentityDocument";

    public async Task<string> SaveAsync(byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        var document = new StoredDocument
        {
            Id = Guid.NewGuid(),
            Purpose = Purpose,
            ContentType = contentType,
            SizeBytes = content.LongLength,
            ProtectedContent = secretProtector.Protect(Purpose, content),
            CreatedAt = timeProvider.GetUtcNow()
        };
        await context.Set<StoredDocument>().AddAsync(document, cancellationToken);
        return document.Id.ToString("N");
    }
}
