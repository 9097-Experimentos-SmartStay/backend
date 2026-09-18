using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Events;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;

public class GuestProfile
{
    private readonly List<IEvent> _domainEvents = [];

    public GuestProfileId Id { get; }
    public UserId? UserId { get; private set; }
    public PersonName Name { get; private set; }
    public EmailAddress? Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public IdentificationDocument? Document { get; private set; }
    public StreetAddress? Address { get; private set; }
    public ProfileStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Required for EF Core materialization without triggering creation domain events
    internal GuestProfile()
    {
        Name = null!;
        Phone = null!;
    }

    public GuestProfile(
        GuestProfileId id,
        PersonName name,
        PhoneNumber phone,
        EmailAddress? email = null,
        IdentificationDocument? document = null,
        StreetAddress? address = null,
        UserId? userId = null)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Email = email;
        Document = document;
        Address = address;
        UserId = userId;
        Status = ProfileStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new GuestProfileCreatedEvent(Id, Name.FullName, Email?.Address, Phone.Value));
    }

    public void LinkToUser(UserId userId, EmailAddress verifiedEmail)
    {
        EnsureActive();

        if (UserId.HasValue)
            throw new BusinessRuleViolationException("Guest profile is already linked to an existing User.");

        UserId = userId;
        Email ??= verifiedEmail;
        UpdatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new GuestLinkedToUserEvent(Id, userId, Email.Address));
    }

    public void CorrectIdentification(IdentificationDocument newDocument, string reason, UserId staffUserId)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainValidationException("A reason must be provided to correct an identification document.");

        var oldDoc = Document ?? throw new BusinessRuleViolationException("No existing document to correct; use SetIdentification.");

        Document = newDocument;
        UpdatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new GuestIdentificationCorrectedEvent(
            Id, oldDoc, newDocument, reason.Trim(), staffUserId, DateTimeOffset.UtcNow));
    }

    public void SetIdentification(IdentificationDocument document)
    {
        EnsureActive();
        if (Document != null)
            throw new BusinessRuleViolationException("Identification is already set. Use CorrectIdentification to change it.");

        Document = document;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContactInformation(PhoneNumber phone, StreetAddress? address)
    {
        EnsureActive();
        Phone = phone;
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = ProfileStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = ProfileStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureActive()
    {
        if (Status == ProfileStatus.Inactive)
            throw new BusinessRuleViolationException("Operation not permitted on an inactive guest profile.");
    }
}
