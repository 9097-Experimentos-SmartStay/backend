using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.BddTests.Support;

public class FakeAccountNotificationService : IAccountNotificationService
{
    public List<string> SentVerificationEmails { get; } = [];
    public List<string> SentPasswordResetLinks { get; } = [];
    public List<string> SentAccountLockedAlerts { get; } = [];
    public List<string> SentPasswordChangedAlerts { get; } = [];
    public List<string> SentPermissionsChangedAlerts { get; } = [];

    public Task SendEmailVerificationAsync(User user, string verificationToken, DateTimeOffset expiresAt)
    {
        SentVerificationEmails.Add(user.Email.Value);
        return Task.CompletedTask;
    }

    public Task SendAccountLockedAsync(User user, DateTimeOffset lockedUntil)
    {
        SentAccountLockedAlerts.Add(user.Email.Value);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(User user, string resetToken, DateTimeOffset expiresAt)
    {
        SentPasswordResetLinks.Add(user.Email.Value);
        return Task.CompletedTask;
    }

    public Task SendPasswordChangedAsync(User user)
    {
        SentPasswordChangedAlerts.Add(user.Email.Value);
        return Task.CompletedTask;
    }

    public Task SendPermissionsChangedAsync(User user)
    {
        SentPermissionsChangedAlerts.Add(user.Email.Value);
        return Task.CompletedTask;
    }
}

public class FakeSecretProtector : ISecretProtector
{
    public string Protect(string purpose, string plaintext) => $"PROT:{purpose}:{plaintext}";
    public string Unprotect(string purpose, string protectedValue)
    {
        var prefix = $"PROT:{purpose}:";
        if (protectedValue.StartsWith(prefix))
            return protectedValue.Substring(prefix.Length);
        return protectedValue;
    }
    public byte[] Protect(string purpose, byte[] plaintext) => plaintext;
    public byte[] Unprotect(string purpose, byte[] protectedValue) => protectedValue;
}

public class FakeDomainEventDispatcher : IDomainEventDispatcher
{
    public List<IEvent> DispatchedEvents { get; } = [];
    public Task DispatchAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        DispatchedEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}

public class FakeDomainEventPublisher : BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices.IDomainEventPublisher
{
    public List<IEvent> PublishedEvents { get; } = [];
    public Task PublishAsync(IReadOnlyCollection<IEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        PublishedEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}

public class FakeBreachedPasswordChecker : IBreachedPasswordChecker
{
    public Task<BreachedPasswordStatus> CheckAsync(string password, CancellationToken cancellationToken = default) =>
        Task.FromResult(BreachedPasswordStatus.NotFound);
}

public class FakeTokenService : ITokenService
{
    public IssuedAccessToken GenerateToken(User user, Guid? rememberedSessionId = null) =>
        new($"fake-jwt-token-{user.Id}", DateTimeOffset.UtcNow.AddHours(1));

    public IssuedMfaChallengeToken GenerateMfaChallengeToken(User user, MfaChallengeKind kind, bool rememberMe) =>
        new(kind, $"fake-mfa-token-{user.Id}-{kind}", DateTimeOffset.UtcNow.AddMinutes(5));
}
