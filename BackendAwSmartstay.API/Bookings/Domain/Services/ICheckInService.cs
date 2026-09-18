using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>A completed check-in with the access code in clear (only for its guest).</summary>
public sealed record CheckInResult(Booking Booking, DigitalCheckIn CheckIn, RoomAccessCode? AccessCode);

/// <summary>Digital check-in use cases (US-08).</summary>
public interface ICheckInService
{
    Task<CheckInResult> Handle(CompleteDigitalCheckInCommand command);

    Task Handle(RequestCheckInAssistanceCommand command);

    /// <summary>The check-in of a booking visible to the requester, or null. The code is only revealed to the guest.</summary>
    Task<CheckInResult?> FindAsync(int bookingId, BookingRequester requester);
}
