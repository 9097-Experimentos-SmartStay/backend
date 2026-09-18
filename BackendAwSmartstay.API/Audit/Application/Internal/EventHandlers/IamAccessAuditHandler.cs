using BackendAwSmartstay.API.Audit.Application.OutboundServices;
using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Audit.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Events;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Audit.Application.Internal.EventHandlers;

/// <summary>
///     Subscribes the Audit context to the access events published by IAM and records one audit entry per event.
///     The e-mail of an administrator who acted on another account is resolved through the IAM ACL facade.
/// </summary>
public class IamAccessAuditHandler(
    IAuditEntryRepository auditEntryRepository,
    IIamContextFacade iamContextFacade,
    IRequestOriginProvider requestOrigin,
    IUnitOfWork unitOfWork) :
    IDomainEventHandler<UserSignedInEvent>,
    IDomainEventHandler<SignInFailedEvent>,
    IDomainEventHandler<UserLockedOutEvent>,
    IDomainEventHandler<UserSignedOutEvent>,
    IDomainEventHandler<UserPasswordResetEvent>,
    IDomainEventHandler<UserPasswordChangedEvent>,
    IDomainEventHandler<UserCreatedEvent>,
    IDomainEventHandler<UserRoleChangedEvent>,
    IDomainEventHandler<UserAssignmentChangedEvent>,
    IDomainEventHandler<UserDeactivatedEvent>,
    IDomainEventHandler<UserActivatedEvent>,
    IDomainEventHandler<MfaEnabledEvent>,
    IDomainEventHandler<MfaVerifiedEvent>,
    IDomainEventHandler<MfaVerificationFailedEvent>,
    IDomainEventHandler<MfaRecoveryCodeUsedEvent>,
    IDomainEventHandler<MfaResetEvent>,
    IDomainEventHandler<UserSignedOutEverywhereEvent>
{
    public Task HandleAsync(MfaEnabledEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.MfaEnabled, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(MfaVerifiedEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.MfaVerified, AuditOutcome.Success, e.UserId, e.Email, e.HotelId,
            AuditDetails.SecondFactor(e.Method.ToString()));

    public Task HandleAsync(MfaVerificationFailedEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.MfaFailed, AuditOutcome.Failure, e.UserId, e.Email, e.HotelId,
            AuditDetails.SecondFactor(e.Method.ToString(), e.Reason.ToString()));

    public Task HandleAsync(MfaRecoveryCodeUsedEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.MfaRecoveryCodeUsed, AuditOutcome.Success, e.UserId, e.Email, e.HotelId,
            AuditDetails.RecoveryCodesLeft(e.RemainingCodes));

    public Task HandleAsync(MfaResetEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.MfaReset, e.ResetByUserId, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserSignedOutEverywhereEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.SignedOutEverywhere, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserSignedInEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.SignInSucceeded, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(SignInFailedEvent e, CancellationToken cancellationToken) =>
        RecordAsync(AuditEntry.Record(e.OccurredOn, AuditAction.SignInFailed, AuditOutcome.Failure,
            e.UserId, e.Email, e.UserId, e.Email, e.HotelId, requestOrigin.ClientIpAddress,
            AuditDetails.FailureReason(e.Reason.ToString())));

    public Task HandleAsync(UserLockedOutEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.AccountLocked, AuditOutcome.Failure, e.UserId, e.Email, e.HotelId,
            AuditDetails.Lock(e.LockedUntil));

    public Task HandleAsync(UserSignedOutEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.SignedOut, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserPasswordResetEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.PasswordReset, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserPasswordChangedEvent e, CancellationToken cancellationToken) =>
        RecordSelfAsync(e.OccurredOn, AuditAction.PasswordChanged, AuditOutcome.Success, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserCreatedEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.UserCreated, e.CreatedByUserId, e.UserId, e.Email, e.HotelId,
            AuditDetails.AssignedRole(e.Role));

    public Task HandleAsync(UserRoleChangedEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.RoleChanged, e.ChangedByUserId, e.UserId, e.Email, e.HotelId,
            AuditDetails.RoleChange(e.PreviousRole, e.NewRole));

    public Task HandleAsync(UserAssignmentChangedEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.AssignmentChanged, e.ChangedByUserId, e.UserId, e.Email,
            e.HotelId ?? e.PreviousHotelId,
            AuditDetails.AssignmentChange(e.PreviousHotelId, e.HotelId, e.PreviousChainId, e.ChainId));

    public Task HandleAsync(UserDeactivatedEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.UserDeactivated, e.DeactivatedByUserId, e.UserId, e.Email, e.HotelId);

    public Task HandleAsync(UserActivatedEvent e, CancellationToken cancellationToken) =>
        RecordByAdministratorAsync(e.OccurredOn, AuditAction.UserActivated, e.ActivatedByUserId, e.UserId, e.Email, e.HotelId);

    private Task RecordSelfAsync(DateTimeOffset occurredOn, AuditAction action, AuditOutcome outcome,
        int userId, string email, int? hotelId, AuditDetails? details = null) =>
        RecordAsync(AuditEntry.Record(occurredOn, action, outcome, userId, email, userId, email, hotelId,
            requestOrigin.ClientIpAddress, details));

    /// <summary>An action performed by an administrator on an account (or by the user themselves when no actor is known).</summary>
    private async Task RecordByAdministratorAsync(DateTimeOffset occurredOn, AuditAction action, int? actorUserId,
        int targetUserId, string targetEmail, int? hotelId, AuditDetails? details = null)
    {
        var actorId = actorUserId ?? targetUserId;
        var actorEmail = actorUserId is null ? targetEmail : await iamContextFacade.FetchEmailByUserId(actorUserId.Value);
        await RecordAsync(AuditEntry.Record(occurredOn, action, AuditOutcome.Success, actorId,
            string.IsNullOrEmpty(actorEmail) ? null : actorEmail, targetUserId, targetEmail, hotelId,
            requestOrigin.ClientIpAddress, details));
    }

    private async Task RecordAsync(AuditEntry entry)
    {
        await auditEntryRepository.AddAsync(entry);
        await unitOfWork.CompleteAsync();
    }
}
