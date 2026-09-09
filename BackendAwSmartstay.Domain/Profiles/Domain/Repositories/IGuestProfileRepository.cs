using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Repositories;

public interface IGuestProfileRepository
{
    Task AddAsync(GuestProfile profile);
    Task<GuestProfile?> FindByIdAsync(GuestProfileId id);
    Task<GuestProfile?> FindByEmailAsync(EmailAddress email);
    Task<GuestProfile?> FindByUserIdAsync(UserId userId);
    Task<GuestProfile?> FindByDocumentAsync(IdentificationDocument document);
    Task<bool> ExistsByEmailAsync(EmailAddress email);
    Task<bool> ExistsByUserIdAsync(UserId userId);
    void Update(GuestProfile profile);
    void Remove(GuestProfile profile);
    Task<IEnumerable<GuestProfile>> ListAsync();
}
