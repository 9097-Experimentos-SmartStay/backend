using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;

public record GuestProfileResource(
    Guid Id,
    int? UserId,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string Phone,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record CreateGuestProfileResource(
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country,
    int? UserId);

public record LinkGuestToUserResource(
    int UserId,
    string VerifiedEmail);

public record SetGuestIdentificationResource(
    DocumentType DocumentType,
    string DocumentNumber);

public record CorrectGuestIdentificationResource(
    DocumentType NewDocumentType,
    string NewDocumentNumber,
    string Reason,
    int? StaffUserId = null); // ignored: the audit identity is taken from the token

public record UpdateGuestContactInformationResource(
    string Phone,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country);
