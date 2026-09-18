using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Domain.Repositories;

/// <summary>One-time recovery codes of the second factor (US-52).</summary>
public interface IMfaRecoveryCodeRepository : IBaseRepository<MfaRecoveryCode>
{
    /// <summary>The codes of the user that were not used yet.</summary>
    Task<IReadOnlyList<MfaRecoveryCode>> ListUnusedByUserAsync(int userId);

    /// <summary>Removes every code of the user (new enrollment or reset).</summary>
    Task RemoveAllOfUserAsync(int userId);
}
