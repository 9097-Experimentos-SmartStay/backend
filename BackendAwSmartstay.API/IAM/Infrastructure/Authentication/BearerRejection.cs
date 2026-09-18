using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Authentication;

/// <summary>
///     Why a request's bearer token was not accepted: the stable code and the English detail of the 401/403, and for a
///     revoked session the reason code (<see cref="SessionRevocationReasons"/>).
/// </summary>
public sealed record BearerRejection(string Code, string Detail, string? Reason = null)
{
    public static readonly BearerRejection Missing =
        new(IamErrorCodes.TokenMissing, "A valid bearer token is required to access this resource.");

    public static readonly BearerRejection Invalid =
        new(IamErrorCodes.TokenInvalid, "The bearer token is invalid.");

    public static readonly BearerRejection Expired =
        new(IamErrorCodes.TokenExpired, "The bearer token has expired. Sign in again.");

    /// <summary>The user's sessions ended after the token was issued (<c>auth.session_revoked</c> + reason).</summary>
    public static BearerRejection SessionRevoked(SessionRevocationReason? reason) =>
        new(IamErrorCodes.SessionRevoked, SessionRevocationReasons.DetailFor(reason), SessionRevocationReasons.CodeFor(reason));

    public static readonly BearerRejection Forbidden =
        new(IamErrorCodes.Forbidden, "You do not have permission to perform this operation.");
}

/// <summary>Authentication failure raised by the IAM session check (TokenValidated), carrying its rejection.</summary>
public sealed class BearerTokenRejectedException(BearerRejection rejection) : Exception(rejection.Detail)
{
    public BearerRejection Rejection { get; } = rejection;
}
