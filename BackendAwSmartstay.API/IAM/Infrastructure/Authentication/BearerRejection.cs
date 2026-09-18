using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Authentication;

/// <summary>Why a request's bearer token was not accepted: the stable code and the English detail of the 401/403.</summary>
public sealed record BearerRejection(string Code, string Detail)
{
    public static readonly BearerRejection Missing =
        new(IamErrorCodes.TokenMissing, "A valid bearer token is required to access this resource.");

    public static readonly BearerRejection Invalid =
        new(IamErrorCodes.TokenInvalid, "The bearer token is invalid.");

    public static readonly BearerRejection Expired =
        new(IamErrorCodes.TokenExpired, "The bearer token has expired. Sign in again.");

    public static readonly BearerRejection Revoked =
        new(IamErrorCodes.TokenRevoked, "The bearer token has been revoked. Sign in again.");

    public static readonly BearerRejection Deactivated =
        new(IamErrorCodes.AccountDeactivated, "The account has been deactivated. Contact the administrator.");

    public static readonly BearerRejection Forbidden =
        new(IamErrorCodes.Forbidden, "You do not have permission to perform this operation.");
}

/// <summary>Authentication failure raised by the IAM session check (TokenValidated), carrying its rejection.</summary>
public sealed class BearerTokenRejectedException(BearerRejection rejection) : Exception(rejection.Detail)
{
    public BearerRejection Rejection { get; } = rejection;
}
