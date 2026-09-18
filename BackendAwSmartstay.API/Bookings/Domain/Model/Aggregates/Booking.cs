using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

/// <summary>
///     Booking aggregate: a room reserved for a guest for a <see cref="DateRange"/>
///     (report: Booking component, <c>BookingStatus {PENDING, CONFIRMED, CANCELLED, COMPLETED}</c>).
/// </summary>
public class Booking
{
    /// <summary>EF Core constructor.</summary>
    protected Booking()
    {
        GuestName = string.Empty;
        GuestEmail = string.Empty;
    }

    private Booking(int roomId, DateRange dates, string guestName, string guestEmail, GuestId? guestId,
        Guid? guestProfileId) : this()
    {
        if (roomId <= 0)
            throw new DomainValidationException("A booking must reference a valid room.");

        RoomId = roomId;
        CheckInDate = dates.CheckIn;
        CheckOutDate = dates.CheckOut;
        GuestName = guestName;
        GuestEmail = guestEmail;
        GuestId = guestId;
        GuestProfileId = guestProfileId;
        Status = BookingStatus.Pending;
    }

    /// <summary>
    ///     Creates a pending booking (report: <c>createBooking(guestId, propertyId, dates)</c>).
    ///     <list type="bullet">
    ///         <item>A guest always books for themselves: the booking references their account and guest profile,
    ///         whatever the request says; a blank name or e-mail defaults to their login e-mail.</item>
    ///         <item>Hotel staff book on behalf of someone: the guest account and profile are optional.</item>
    ///     </list>
    ///     Availability (no overbooking) is checked beforehand by <c>RoomAvailabilityService</c>.
    /// </summary>
    public static Booking Create(BookingRequester requester, int roomId, DateRange dates,
        string? guestName, string? guestEmail, int? guestUserId = null, Guid? guestProfileId = null)
    {
        if (requester.IsGuest)
        {
            var fallback = requester.DisplayName ?? string.Empty;
            return new Booking(roomId, dates,
                string.IsNullOrWhiteSpace(guestName) ? fallback : guestName,
                string.IsNullOrWhiteSpace(guestEmail) ? fallback : guestEmail,
                new GuestId(requester.UserId),
                requester.GuestProfileId);
        }

        return new Booking(roomId, dates, guestName ?? string.Empty, guestEmail ?? string.Empty,
            guestUserId is > 0 ? new GuestId(guestUserId.Value) : null,
            guestProfileId);
    }

    public int Id { get; }

    /// <summary>The booked room.</summary>
    public int RoomId { get; private set; }

    /// <summary>The logical external identifier of the associated guest profile.</summary>
    public Guid? GuestProfileId { get; private set; }

    /// <summary>
    ///     The guest account that owns the booking. Always set for bookings made by a guest; optional for bookings
    ///     made at the desk for someone without an account.
    /// </summary>
    public GuestId? GuestId { get; private set; }

    public string GuestName { get; private set; }

    public string GuestEmail { get; private set; }

    public DateTime CheckInDate { get; private set; }

    public DateTime CheckOutDate { get; private set; }

    public BookingStatus Status { get; private set; }

    /// <summary>The stay dates.</summary>
    public DateRange Dates => new(CheckInDate, CheckOutDate);

    /// <summary>Number of nights covered by the booking.</summary>
    public int Nights => Dates.Nights;

    /// <summary>A booking can be paid while it is Pending or Confirmed.</summary>
    public bool CanBePaid => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    /// <summary>
    ///     A guest owns a booking made with their account or attached to their guest profile.
    /// </summary>
    public bool IsOwnedBy(BookingRequester requester) =>
        requester.IsGuest
        && ((GuestId is not null && GuestId.Value == requester.UserId)
            || (GuestProfileId.HasValue && GuestProfileId == requester.GuestProfileId));

    /// <summary>Hotel staff see every booking; a guest only their own.</summary>
    public bool IsVisibleTo(BookingRequester requester) => !requester.IsGuest || IsOwnedBy(requester);

    /// <summary>
    ///     R5 / US-11: this Confirmed booking is the current stay in <paramref name="roomId"/> on
    ///     <paramref name="day"/>; its guest may control the room's devices.
    /// </summary>
    public bool IsConfirmedStayIn(int roomId, DateTime day) =>
        Status == BookingStatus.Confirmed && RoomId == roomId && Dates.Includes(day);

    /// <summary>Confirms the booking. Confirming twice is a no-op.</summary>
    /// <exception cref="InvalidBookingTransitionException">The booking is cancelled or completed.</exception>
    public void Confirm()
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.Completed)
            throw new InvalidBookingTransitionException(
                $"A {Status.ToString().ToLowerInvariant()} booking cannot be confirmed.");

        Status = BookingStatus.Confirmed;
    }

    /// <summary>
    ///     Cancels the booking on behalf of <paramref name="requester"/>. A guest can only cancel their own
    ///     booking; for anyone else's the booking does not exist. Cancelling twice is a no-op.
    /// </summary>
    /// <exception cref="BookingNotFoundException">A guest tries to cancel a booking that is not theirs.</exception>
    /// <exception cref="InvalidBookingTransitionException">The booking is already completed.</exception>
    public void Cancel(BookingRequester requester)
    {
        if (!IsVisibleTo(requester))
            throw new BookingNotFoundException(Id);
        if (Status == BookingStatus.Completed)
            throw new InvalidBookingTransitionException("A completed booking cannot be cancelled.");

        Status = BookingStatus.Cancelled;
    }
}

/// <summary>
///     Enumeration of possible booking statuses.
/// </summary>
public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public static class BookingStatusExtensions
{
    /// <summary>Active bookings hold the room: Pending or Confirmed (R1).</summary>
    public static bool IsActive(this BookingStatus status) => status is BookingStatus.Pending or BookingStatus.Confirmed;
}
