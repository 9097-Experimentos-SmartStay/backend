using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Domain.Repositories;

/// <summary>Single-use account tokens (e-mail verification, password reset).</summary>
public interface IAccountTokenRepository : IBaseRepository<AccountToken>
{
    /// <summary>The token with that purpose and hash, or null.</summary>
    Task<AccountToken?> FindByHashAsync(AccountTokenPurpose purpose, string tokenHash);

    /// <summary>The not-yet-used, not-superseded tokens of a user for a purpose.</summary>
    Task<IReadOnlyList<AccountToken>> ListOutstandingAsync(int userId, AccountTokenPurpose purpose);
}
