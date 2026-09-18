namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>An opaque random token: <see cref="Value"/> goes to the user, only <see cref="Hash"/> is stored.</summary>
public sealed record SecureToken(string Value, string Hash);

/// <summary>Creates unguessable tokens for links and refresh tokens, and hashes presented ones for lookup.</summary>
public interface ISecureTokenGenerator
{
    SecureToken Generate();

    string Hash(string tokenValue);
}
