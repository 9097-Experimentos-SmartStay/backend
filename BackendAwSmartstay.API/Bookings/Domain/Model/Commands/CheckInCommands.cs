using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>US-08: the guest completes the digital check-in of their booking with their identity document.</summary>
/// <param name="BookingId">The booking.</param>
/// <param name="Requester">The guest (owner of the booking).</param>
/// <param name="DocumentType">DNI, passport or carné de extranjería.</param>
/// <param name="DocumentNumber">Document number.</param>
/// <param name="Nationality">ISO 3166-1 alpha-2 code.</param>
/// <param name="DocumentContent">Photo or scan of the document (JPEG, PNG or PDF, ≤ 5 MB).</param>
public record CompleteDigitalCheckInCommand(int BookingId, BookingRequester Requester, IdentityDocumentType DocumentType,
    string DocumentNumber, string Nationality, byte[] DocumentContent);

/// <summary>US-08 scenario 3: the guest asks the front desk for help with the check-in.</summary>
public record RequestCheckInAssistanceCommand(int BookingId, BookingRequester Requester, string? Message);
