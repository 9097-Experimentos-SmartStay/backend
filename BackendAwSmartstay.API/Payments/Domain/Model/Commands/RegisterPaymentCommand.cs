using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Domain.Model.Commands;

/// <summary>
///     US-07 scenario 5: hotel staff register the payment of a booking. The amount is not part of the command: it is
///     always the booking's total.
/// </summary>
/// <param name="BookingId">The booking paid.</param>
/// <param name="Method">How it was paid.</param>
/// <param name="OperationNumber">Operation number (required except for cash).</param>
/// <param name="Note">Optional note.</param>
/// <param name="StaffUserId">Who registers it.</param>
/// <param name="StaffHotelId">Their hotel (admin, reception).</param>
/// <param name="AllHotels">True for a chain administrator.</param>
public record RegisterPaymentCommand(
    int BookingId,
    PaymentMethod Method,
    string? OperationNumber,
    string? Note,
    int StaffUserId,
    int? StaffHotelId,
    bool AllHotels);
