using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
/// User Aggregate Root.
/// Represents a registered user within the identity context.
/// </summary>
public class User
{
    public User(string email, string passwordHash, string role,
        UserStatus status = UserStatus.Active,
        int? hotelId = null,
        int? chainId = null,
        int tokenVersion = 0)
    {
        Email = new Email(email);
        PasswordHash = passwordHash;
        Role = new Role(role);
        Status = status;
        HotelId = hotelId;
        ChainId = chainId;
        TokenVersion = tokenVersion;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// EF Core constructor. Do not use directly in domain logic.
    /// </summary>
    protected User()
    {
        Email = null!; // EF populates this via reflection after materialization
        PasswordHash = string.Empty;
        Role = null!; // EF populates this via reflection after materialization
        Status = UserStatus.Active;
        TokenVersion = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    /// <summary>The login identifier of the account (US-01/US-02).</summary>
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; }
    public int? HotelId { get; private set; }
    public int? ChainId { get; private set; }
    public int TokenVersion { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User UpdateEmail(string email)
    {
        Email = new Email(email);
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("Password hash cannot be empty.");
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User AssignRole(string newRole)
    {
        Role = new Role(newRole);
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>Revokes every token issued so far (password change, deactivation).</summary>
    public User IncrementTokenVersion() => StartNewSession();

    public User UpdateHotelId(int? hotelId)
    {
        HotelId = hotelId;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User UpdateChainId(int? chainId)
    {
        ChainId = chainId;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    ///     Decides whether an access token issued with <paramref name="tokenVersion"/> still represents a valid
    ///     session: the account must be active and the token must belong to the current session generation.
    /// </summary>
    public UserSession GetSession(int tokenVersion)
    {
        if (Status == UserStatus.Inactive) return new UserSession(UserSessionStatus.Inactive);
        if (tokenVersion != TokenVersion) return new UserSession(UserSessionStatus.Revoked);
        return new UserSession(UserSessionStatus.Valid, Role.Value, HotelId, ChainId);
    }

    private User StartNewSession()
    {
        TokenVersion++;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}