using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Events;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
/// User Aggregate Root.
/// Represents a registered user within the identity context: credentials, role and scope, e-mail verification
/// (US-01), the temporary lock after repeated failed sign-ins (US-02) and the session generation that revokes
/// access tokens. Access-related facts are recorded as domain events (US-03 access audit).
/// </summary>
public class User : IHasDomainEvents
{
    private readonly List<IEvent> _domainEvents = [];

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

    /// <summary>
    ///     Registers a new account (US-01 self-registration, or US-03 an administrator creating a staff user).
    ///     The e-mail starts unverified; a verification link is sent by the application layer.
    /// </summary>
    /// <param name="name">First and last name of the owner.</param>
    /// <param name="email">Login identifier.</param>
    /// <param name="passwordHash">Hash of the chosen password.</param>
    /// <param name="role">Assigned role.</param>
    /// <param name="hotelId">Hotel the account belongs to (staff).</param>
    /// <param name="chainId">Chain the account belongs to.</param>
    /// <param name="createdByUserId">Administrator who created it; null for self-registration.</param>
    /// <param name="now">Current time.</param>
    public static User Register(PersonName name, Email email, string passwordHash, Role role,
        int? hotelId, int? chainId, int? createdByUserId, DateTimeOffset now)
    {
        var user = new User(email.Value, passwordHash, role.Value, hotelId: hotelId, chainId: chainId);
        user.FirstName = name.FirstName;
        user.LastName = name.LastName;
        user.CreatedAt = now.UtcDateTime;
        user.UpdatedAt = now.UtcDateTime;
        user._pendingCreation = (createdByUserId, now);
        return user;
    }

    // UserCreatedEvent needs the generated id: it is recorded when the unit of work collects the events.
    private (int? CreatedBy, DateTimeOffset At)? _pendingCreation;

    public int Id { get; private set; }
    /// <summary>The login identifier of the account (US-01/US-02).</summary>
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; }
    public int? HotelId { get; private set; }
    public int? ChainId { get; private set; }
    public int TokenVersion { get; private set; }

    /// <summary>Why the last session generation started (null before any revocation): told to revoked tokens.</summary>
    public SessionRevocationReason? SessionRevocationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>First name (null for accounts created before names were required).</summary>
    public string? FirstName { get; private set; }

    /// <summary>Last name (null for accounts created before names were required).</summary>
    public string? LastName { get; private set; }

    /// <summary>True once the owner opened the verification link sent to the e-mail (US-01).</summary>
    public bool EmailVerified { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    /// <summary>Consecutive failed sign-ins since the last successful one or the last lock (US-02).</summary>
    public int FailedSignInAttempts { get; private set; }

    /// <summary>End of the current temporary lock, if any (US-02 scenario 3).</summary>
    public DateTimeOffset? LockedUntil { get; private set; }

    /// <summary>True once the user enrolled an authenticator app (US-52).</summary>
    public bool MfaEnabled { get; private set; }

    /// <summary>When two-factor authentication was enabled.</summary>
    public DateTimeOffset? MfaEnabledAt { get; private set; }

    /// <summary>The TOTP secret of the enrolled authenticator, encrypted by the application (never in clear).</summary>
    public string? MfaSecretProtected { get; private set; }

    /// <summary>The secret of an enrollment in progress (shown in the QR code, not confirmed yet), encrypted.</summary>
    public string? MfaPendingSecretProtected { get; private set; }

    /// <summary>Last TOTP time step accepted: a code of that step or an earlier one is a replay.</summary>
    public long? MfaLastUsedTimeStep { get; private set; }

    /// <summary>
    ///     US-52 scenario 1: a staff account without an authenticator must enroll one before it gets access.
    /// </summary>
    public bool RequiresMfaEnrollment => Role.RequiresMultiFactorAuthentication && !MfaEnabled;

    public IReadOnlyCollection<IEvent> DomainEvents
    {
        get
        {
            // The creation event is recorded once the account has its database id.
            if (_pendingCreation is { } creation && Id > 0)
            {
                _domainEvents.Insert(0, new UserCreatedEvent(Id, Email.Value, HotelId, Role.Value, creation.CreatedBy, creation.At));
                _pendingCreation = null;
            }
            return _domainEvents.AsReadOnly();
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public User UpdateEmail(string email)
    {
        Email = new Email(email);
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException(IamErrorCodes.InternalInvariant, "Password hash cannot be empty.");
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    ///     Changes the role (US-03 scenario 2). The claims of a token are its permissions, so a new role ends every
    ///     session of the user at once (all access and refresh tokens): their next request gets 401
    ///     <c>auth.session_revoked</c> (<c>role_changed</c>) and they sign in again with the new permissions.
    /// </summary>
    public User AssignRole(string newRole, int? changedByUserId = null, DateTimeOffset? now = null)
    {
        var previous = Role;
        Role = new Role(newRole);
        UpdatedAt = DateTime.UtcNow;
        if (previous != Role)
        {
            StartNewSession(Enums.SessionRevocationReason.RoleChanged);
            _domainEvents.Add(new UserRoleChangedEvent(Id, Email.Value, HotelId, previous.Value, Role.Value,
                changedByUserId, now ?? DateTimeOffset.UtcNow));
        }
        return this;
    }

    /// <summary>
    ///     Reassigns the user's hotel and chain. Like a role change, a different scope ends every session of the user
    ///     (their tokens carry the old one).
    /// </summary>
    /// <returns>True when the hotel or the chain actually changed.</returns>
    public bool ChangeAssignment(int? hotelId, int? chainId, int? changedByUserId, DateTimeOffset now)
    {
        if (hotelId == HotelId && chainId == ChainId) return false;
        var previousHotel = HotelId;
        var previousChain = ChainId;
        HotelId = hotelId;
        ChainId = chainId;
        StartNewSession(Enums.SessionRevocationReason.AssignmentChanged);
        _domainEvents.Add(new UserAssignmentChangedEvent(Id, Email.Value, previousHotel, HotelId, previousChain, ChainId,
            changedByUserId, now));
        return true;
    }

    /// <summary>
    ///     Deactivates the account (US-03 scenario 3): the user loses access immediately (every token is revoked)
    ///     but the account and its history are kept.
    /// </summary>
    public User Deactivate(int? deactivatedByUserId = null, DateTimeOffset? now = null)
    {
        if (Status == UserStatus.Inactive) return this;
        Status = UserStatus.Inactive;
        StartNewSession(Enums.SessionRevocationReason.Deactivated);
        _domainEvents.Add(new UserDeactivatedEvent(Id, Email.Value, HotelId, deactivatedByUserId, now ?? DateTimeOffset.UtcNow));
        return this;
    }

    public User Activate(int? activatedByUserId = null, DateTimeOffset? now = null)
    {
        if (Status == UserStatus.Active) return this;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new UserActivatedEvent(Id, Email.Value, HotelId, activatedByUserId, now ?? DateTimeOffset.UtcNow));
        return this;
    }

    /// <summary>Revokes every token issued so far after a password change.</summary>
    public User IncrementTokenVersion() => StartNewSession(Enums.SessionRevocationReason.PasswordChanged);

    /// <summary>
    ///     D2: a hotel administrator administers a single hotel. Taking charge of the hotel they registered is
    ///     only possible while they have none (or it is the same hotel). The new hotel ends the admin's sessions
    ///     (their token has no hotel): they sign in again to manage it.
    /// </summary>
    public User TakeChargeOfHotel(int hotelId, DateTimeOffset? now = null)
    {
        if (!Role.Value.Equals(UserRoles.Admin, StringComparison.Ordinal))
            throw new BusinessRuleViolationException(IamErrorCodes.NotHotelAdministrator, "Only a hotel administrator takes charge of a single hotel.");
        if (HotelId is not null && HotelId != hotelId)
            throw new BusinessRuleViolationException(IamErrorCodes.AdminAlreadyHasHotel, "A hotel administrator manages a single hotel and already has one.");

        ChangeAssignment(hotelId, ChainId, Id, now ?? DateTimeOffset.UtcNow);
        return this;
    }

    /// <summary>
    ///     Decides whether an access token issued with <paramref name="tokenVersion"/> still represents a valid
    ///     session: the account must be active and the token must belong to the current session generation.
    /// </summary>
    public UserSession GetSession(int tokenVersion)
    {
        if (Status == UserStatus.Inactive)
            return new UserSession(UserSessionStatus.Inactive, RevocationReason: Enums.SessionRevocationReason.Deactivated);
        if (tokenVersion != TokenVersion)
            return new UserSession(UserSessionStatus.Revoked, RevocationReason: SessionRevocationReason);
        return new UserSession(UserSessionStatus.Valid, Role.Value, HotelId, ChainId);
    }

    // ── Sign-in (US-02) ─────────────────────────────────────────────────────

    /// <summary>True while the temporary lock is in effect.</summary>
    public bool IsLockedOut(DateTimeOffset now) => LockedUntil is { } until && now < until;

    /// <summary>
    ///     Records a failed sign-in with a wrong password. Reaching <see cref="SignInLockoutPolicy.MaxConsecutiveFailures"/>
    ///     consecutive failures locks the account for the policy's duration.
    /// </summary>
    /// <returns>True when this failure started a lock (the owner must be notified).</returns>
    public bool RegisterFailedSignIn(SignInLockoutPolicy policy, DateTimeOffset now)
    {
        if (IsLockedOut(now))
        {
            RejectSignInWhileLocked(now);
            return false;
        }

        return RegisterFailure(new SignInFailedEvent(Id, Email.Value, HotelId, SignInFailureReason.WrongPassword, now), policy, now);
    }

    /// <summary>Counts a failed credential (password or second factor) toward the temporary lock.</summary>
    private bool RegisterFailure(IEvent failure, SignInLockoutPolicy policy, DateTimeOffset now)
    {
        FailedSignInAttempts++;
        _domainEvents.Add(failure);

        if (FailedSignInAttempts < policy.MaxConsecutiveFailures) return false;

        LockedUntil = now + policy.LockoutDuration;
        FailedSignInAttempts = 0;
        _domainEvents.Add(new UserLockedOutEvent(Id, Email.Value, HotelId, LockedUntil.Value, now));
        return true;
    }

    /// <summary>Records an attempt rejected because the account is locked (the password is not even checked).</summary>
    public void RejectSignInWhileLocked(DateTimeOffset now) =>
        _domainEvents.Add(new SignInFailedEvent(Id, Email.Value, HotelId, SignInFailureReason.AccountLocked, now));

    /// <summary>Records an attempt with the right password on a deactivated account.</summary>
    public void RejectSignInWhileDeactivated(DateTimeOffset now) =>
        _domainEvents.Add(new SignInFailedEvent(Id, Email.Value, HotelId, SignInFailureReason.AccountDeactivated, now));

    /// <summary>
    ///     US-01: an account signs in only once its e-mail is verified. The attempt had the right password, so it
    ///     does not count toward the temporary lock.
    /// </summary>
    public void RejectSignInWithUnverifiedEmail(DateTimeOffset now) =>
        _domainEvents.Add(new SignInFailedEvent(Id, Email.Value, HotelId, SignInFailureReason.EmailNotVerified, now));

    /// <summary>A successful sign-in resets the consecutive failures and ends an expired lock.</summary>
    public void RegisterSuccessfulSignIn(DateTimeOffset now)
    {
        if (IsLockedOut(now))
            throw new BusinessRuleViolationException(IamErrorCodes.AccountLocked, "A locked account cannot sign in.");
        FailedSignInAttempts = 0;
        LockedUntil = null;
        _domainEvents.Add(new UserSignedInEvent(Id, Email.Value, HotelId, now));
    }

    /// <summary>Records that the user signed out of a remembered session.</summary>
    public void SignOut(DateTimeOffset now) =>
        _domainEvents.Add(new UserSignedOutEvent(Id, Email.Value, HotelId, now));

    // ── Two-factor authentication, TOTP (US-52) ─────────────────────────────

    /// <summary>
    ///     Starts (or restarts) the enrollment of an authenticator app with a new secret, encrypted by the caller.
    ///     The secret only becomes the user's second factor once a code generated from it is confirmed.
    /// </summary>
    public void StartMfaEnrollment(string protectedSecret)
    {
        if (MfaEnabled)
            throw new BusinessRuleViolationException(IamErrorCodes.MfaAlreadyEnabled, "Two-factor authentication is already enabled for this account.");
        if (string.IsNullOrWhiteSpace(protectedSecret))
            throw new DomainValidationException(IamErrorCodes.InternalInvariant, "The enrollment needs a secret.");
        MfaPendingSecretProtected = protectedSecret;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Confirms the enrollment with a code of the authenticator app (<paramref name="pendingSecret"/> is the
    ///     decrypted <see cref="MfaPendingSecretProtected"/>). A wrong code counts toward the temporary lock.
    /// </summary>
    public MfaCodeOutcome ConfirmMfaEnrollment(TotpSecret pendingSecret, string code, SignInLockoutPolicy policy, DateTimeOffset now)
    {
        EnsureMfaEnrollmentInProgress();

        var step = TotpAlgorithm.MatchingTimeStep(pendingSecret, code, now);
        if (step is null)
            return RejectSecondFactor(MfaMethod.AuthenticatorCode, "InvalidCode", policy, now);

        MfaSecretProtected = MfaPendingSecretProtected;
        MfaPendingSecretProtected = null;
        MfaEnabled = true;
        MfaEnabledAt = now;
        MfaLastUsedTimeStep = step;
        UpdatedAt = now.UtcDateTime;
        _domainEvents.Add(new MfaEnabledEvent(Id, Email.Value, HotelId, now));
        return MfaCodeOutcome.Accepted;
    }

    /// <summary>
    ///     Verifies a code of the enrolled authenticator (<paramref name="secret"/> is the decrypted
    ///     <see cref="MfaSecretProtected"/>). Codes of an already used time step are replays and are rejected.
    ///     Every rejection counts toward the temporary lock.
    /// </summary>
    public MfaCodeOutcome VerifyMfaCode(TotpSecret secret, string code, SignInLockoutPolicy policy, DateTimeOffset now)
    {
        EnsureMfaEnabled();

        var step = TotpAlgorithm.MatchingTimeStep(secret, code, now);
        if (step is null)
            return RejectSecondFactor(MfaMethod.AuthenticatorCode, "InvalidCode", policy, now);
        if (MfaLastUsedTimeStep is { } last && step <= last)
            return RejectSecondFactor(MfaMethod.AuthenticatorCode, "CodeAlreadyUsed", policy, now) == MfaCodeOutcome.LockStarted
                ? MfaCodeOutcome.LockStarted
                : MfaCodeOutcome.Replayed;

        MfaLastUsedTimeStep = step;
        _domainEvents.Add(new MfaVerifiedEvent(Id, Email.Value, HotelId, MfaMethod.AuthenticatorCode, now));
        return MfaCodeOutcome.Accepted;
    }

    /// <summary>Records that a one-time recovery code of the user was redeemed.</summary>
    public void AcceptRecoveryCode(int remainingCodes, DateTimeOffset now)
    {
        EnsureMfaEnabled();
        _domainEvents.Add(new MfaRecoveryCodeUsedEvent(Id, Email.Value, HotelId, remainingCodes, now));
        _domainEvents.Add(new MfaVerifiedEvent(Id, Email.Value, HotelId, MfaMethod.RecoveryCode, now));
    }

    /// <summary>A recovery code that is unknown or already used. Counts toward the temporary lock.</summary>
    public MfaCodeOutcome RejectRecoveryCode(SignInLockoutPolicy policy, DateTimeOffset now)
    {
        EnsureMfaEnabled();
        return RejectSecondFactor(MfaMethod.RecoveryCode, "InvalidRecoveryCode", policy, now);
    }

    /// <summary>
    ///     US-52 scenario 4: an administrator removes the second factor (lost phone). Every session ends and the
    ///     user must enroll a new authenticator at the next sign-in. Like a password reset, the administrator's
    ///     intervention also lifts a temporary lock (a user who lost the phone has usually locked the account).
    /// </summary>
    public void ResetMfa(int? resetByUserId, DateTimeOffset now)
    {
        FailedSignInAttempts = 0;
        LockedUntil = null;
        MfaEnabled = false;
        MfaEnabledAt = null;
        MfaSecretProtected = null;
        MfaPendingSecretProtected = null;
        MfaLastUsedTimeStep = null;
        StartNewSession(Enums.SessionRevocationReason.MfaReset);
        _domainEvents.Add(new MfaResetEvent(Id, Email.Value, HotelId, resetByUserId, now));
    }

    /// <summary>Closes every session on every device: all access and refresh tokens stop working.</summary>
    public void SignOutEverywhere(DateTimeOffset now)
    {
        StartNewSession(Enums.SessionRevocationReason.SignedOutEverywhere);
        _domainEvents.Add(new UserSignedOutEverywhereEvent(Id, Email.Value, HotelId, now));
    }

    private MfaCodeOutcome RejectSecondFactor(MfaMethod method, string reason, SignInLockoutPolicy policy, DateTimeOffset now) =>
        RegisterFailure(new MfaVerificationFailedEvent(Id, Email.Value, HotelId, method, reason, now), policy, now)
            ? MfaCodeOutcome.LockStarted
            : MfaCodeOutcome.Rejected;

    /// <summary>An enrollment was started (QR code shown) and MFA is not enabled yet.</summary>
    public void EnsureMfaEnrollmentInProgress()
    {
        if (MfaEnabled)
            throw new BusinessRuleViolationException(IamErrorCodes.MfaAlreadyEnabled, "Two-factor authentication is already enabled for this account.");
        if (MfaPendingSecretProtected is null)
            throw new BusinessRuleViolationException(IamErrorCodes.MfaEnrollmentNotStarted, "Start the two-factor enrollment first to get the QR code.");
    }

    /// <summary>The account has an enrolled authenticator.</summary>
    public void EnsureMfaEnabled()
    {
        if (!MfaEnabled)
            throw new BusinessRuleViolationException(IamErrorCodes.MfaNotEnabled, "Two-factor authentication is not enabled for this account.");
    }

    // ── E-mail verification (US-01) ─────────────────────────────────────────

    /// <summary>Marks the e-mail as verified (idempotent).</summary>
    public void VerifyEmail(DateTimeOffset now)
    {
        if (EmailVerified) return;
        EmailVerified = true;
        EmailVerifiedAt = now;
        UpdatedAt = now.UtcDateTime;
    }

    // ── Passwords (US-04) ───────────────────────────────────────────────────

    /// <summary>
    ///     Sets the password chosen through the recovery link (US-04 scenario 4): every session is revoked, the
    ///     temporary lock is lifted and, since the link reached the inbox, the e-mail counts as verified.
    /// </summary>
    public void ResetPassword(string newPasswordHash, DateTimeOffset now)
    {
        UpdatePasswordHash(newPasswordHash);
        FailedSignInAttempts = 0;
        LockedUntil = null;
        VerifyEmail(now);
        StartNewSession(Enums.SessionRevocationReason.PasswordReset);
        _domainEvents.Add(new UserPasswordResetEvent(Id, Email.Value, HotelId, now));
    }

    /// <summary>Changes the password knowing the current one: every other session is revoked.</summary>
    public void ChangePassword(string newPasswordHash, DateTimeOffset now)
    {
        UpdatePasswordHash(newPasswordHash);
        StartNewSession(Enums.SessionRevocationReason.PasswordChanged);
        _domainEvents.Add(new UserPasswordChangedEvent(Id, Email.Value, HotelId, now));
    }

    /// <summary>Starts a new session generation: every access and refresh token issued before stops working.</summary>
    private User StartNewSession(SessionRevocationReason reason)
    {
        TokenVersion++;
        SessionRevocationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}
