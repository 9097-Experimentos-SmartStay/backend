using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.Commands;

public record CreateGuestProfileCommand(
    string FirstName,
    string LastName,
    string Phone,
    string? Email = null,
    DocumentType? DocumentType = null,
    string? DocumentNumber = null,
    string? Street = null,
    string? Number = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    UserId? UserId = null);

public record LinkGuestToUserCommand(
    GuestProfileId GuestProfileId,
    UserId UserId,
    EmailAddress VerifiedEmail);

public record SetGuestIdentificationCommand(
    GuestProfileId GuestProfileId,
    IdentificationDocument Document);

public record CorrectGuestIdentificationCommand(
    GuestProfileId GuestProfileId,
    IdentificationDocument NewDocument,
    string Reason,
    UserId StaffUserId);

public record UpdateGuestContactInformationCommand(
    GuestProfileId GuestProfileId,
    PhoneNumber Phone,
    StreetAddress? Address);

public record DeactivateGuestProfileCommand(
    GuestProfileId GuestProfileId);

public record ActivateGuestProfileCommand(
    GuestProfileId GuestProfileId);
