using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

/// <summary>
///     Booking aggregate: a room of a hotel reserved for a guest for a <see cref="DateRange"/>.
///     <para>Lifecycle (D1, US-51, US-07, US-08):</para>
///     <list type="bullet">
///         <item><b>Pending</b>: created once the room was found free (under the room row lock). It holds the room
///         until its payment deadline (<see cref="PaymentDueAt"/>); without a payment it expires (cancelled).</item>
///         <item><b>Confirmed</b>: its payment was registered. Only a payment confirms a booking.</item>
///         <item><b>CheckedIn</b>: the guest completed the digital check-in; the stay is in progress.</item>
///         <item><b>Cancelled</b>: by the guest, by the hotel or by the payment deadline. Frees its nights.</item>
///         <item><b>Completed</b>: reserved for the check-out (not implemented yet).</item>
///     </list>
/// </summary>
public class Booking : IHasDomainEvents
{
    private readonly List<IEvent> _domainEvents = [];
    private DateTimeOffset? _pendingCreationAt;

    /// <summary>EF Core constructor.</summary>
    protected Booking()
    {
        GuestName = string.Empty;
        GuestEmail = string.Empty;
        Code = null!;
    }

    public int Id { get; private set; }

    /// <summary>Unique human-readable code (US-51 scenario 2).</summary>
    public BookingCode Code { get; private set; }

    /// <summary>Hotel of the booked room (R4: staff only see the bookings of their hotel).</summary>
    public int HotelId { get; private set; }

    /// <summary>The booked room.</summary>
    public int RoomId { get; private set; }

    /// <summary>The logical external identifier of the associated guest profile.</summary>
    public Guid? GuestProfileId { get; private set; }

    /// <summary>
    ///     The guest account that owns the booking. Always set for bookings made by a guest; optional for bookings
    ///     taken by the staff for someone without an account.
    /// </summary>
    public GuestId? GuestId { get; private set; }

    public string GuestName { get; private set; }

    /// <summary>Where the booking e-mails go.</summary>
    public string GuestEmail { get; private set; }

    public string? GuestPhone { get; private set; }

    public DateTime CheckInDate { get; private set; }

    public DateTime CheckOutDate { get; private set; }

    /// <summary>Price per night agreed when the booking was made (the room price may change later).</summary>
    public decimal PricePerNight { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>A Pending booking without a payment by this moment expires.</summary>
    public DateTimeOffset? PaymentDueAt { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public CancellationReason? CancellationReason { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }

    /// <summary>The stay dates.</summary>
    public DateRange Dates => new(CheckInDate, CheckOutDate);

    /// <summary>Number of nights covered by the booking.</summary>
    public int Nights => Dates.Nights;

    /// <summary>What the stay costs: price per night × nights. The client never decides the amount.</summary>
    public decimal TotalPrice => decimal.Round(PricePerNight * Nights, 2, MidpointRounding.AwayFromZero);

    /// <summary>Only a Pending booking waits for its payment.</summary>
    public bool CanBePaid => Status == BookingStatus.Pending;

    public IReadOnlyCollection<IEvent> DomainEvents
    {
        get
        {
            // The creation event carries the generated id and is recorded once the booking has it.
            if (_pendingCreationAt is { } at && Id > 0)
            {
                _domainEvents.Insert(0, new BookingCreatedEvent(Id, Code.Value, HotelId, RoomId, PaymentDueAt!.Value, at));
                _pendingCreationAt = null;
            }
            return _domainEvents.AsReadOnly();
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    ///     Places a Pending booking (US-51 guest self-service, US-07 scenario 2 reservation taken by the staff).
    ///     Availability (no overbooking, R1) is checked beforehand by <c>RoomAvailabilityService</c> under the room
    ///     row lock.
    ///     <list type="bullet">
    ///         <item>A guest books for themselves: the booking references their account and guest profile.</item>
    ///         <item>Staff book rooms of their hotel for a guest identified by <paramref name="contact"/>, with or
    ///         without an account.</item>
    ///         <item>The check-in cannot be in the past; a room under maintenance cannot be booked.</item>
    ///         <item>The hotel must accept bookings (it has payment methods, US-53).</item>
    ///     </list>
    /// </summary>
    public static Booking Place(BookingRequester requester, RoomOffer room, DateRange dates, GuestContact contact,
        int? guestUserId, Guid? guestProfileId, DateTime hotelToday, DateTimeOffset now, TimeSpan paymentHold)
    {
        if (!requester.IsGuest && !requester.OperatesHotel(room.HotelId))
            throw new BookingOutsideHotelScopeException();
        dates.EnsureNotInThePast(hotelToday);
        EnsureHotelAcceptsBookings(room);
        EnsureBookable(room);
        if (paymentHold <= TimeSpan.Zero)
            throw new DomainValidationException(BookingErrorCodes.InternalInvariant, "The payment hold must be positive.");

        var booking = new Booking
        {
            Code = BookingCode.New(),
            HotelId = room.HotelId,
            RoomId = room.RoomId,
            CheckInDate = dates.CheckIn,
            CheckOutDate = dates.CheckOut,
            PricePerNight = room.PricePerNight,
            GuestName = contact.Name,
            GuestEmail = contact.Email,
            GuestPhone = contact.Phone,
            GuestId = requester.IsGuest ? new GuestId(requester.UserId) : guestUserId is > 0 ? new GuestId(guestUserId.Value) : null,
            GuestProfileId = requester.IsGuest ? requester.GuestProfileId : guestProfileId,
            Status = BookingStatus.Pending,
            CreatedAt = now,
            PaymentDueAt = now + paymentHold,
            _pendingCreationAt = now
        };
        return booking;
    }

    /// <summary>A guest owns a booking made with their account or attached to their guest profile.</summary>
    public bool IsOwnedBy(BookingRequester requester) =>
        requester.IsGuest
        && ((GuestId is not null && GuestId.Value == requester.UserId)
            || (GuestProfileId.HasValue && GuestProfileId == requester.GuestProfileId));

    /// <summary>A guest sees their own bookings; staff those of their hotel; a chain administrator all (R4).</summary>
    public bool IsVisibleTo(BookingRequester requester) =>
        requester.IsGuest ? IsOwnedBy(requester) : requester.OperatesHotel(HotelId);

    /// <summary>
    ///     R5 / US-11: the guest of this booking is staying in <paramref name="roomId"/> on <paramref name="day"/>
    ///     (Confirmed, or already checked in), so they may control the room's devices.
    /// </summary>
    public bool IsConfirmedStayIn(int roomId, DateTime day) =>
        Status is BookingStatus.Confirmed or BookingStatus.CheckedIn && RoomId == roomId && Dates.Includes(day);

    /// <summary>Confirms the booking once its payment is registered (D1). Confirming twice is a no-op.</summary>
    /// <exception cref="InvalidBookingTransitionException">The booking is not Pending.</exception>
    public void Confirm(DateTimeOffset now)
    {
        if (Status == BookingStatus.Confirmed) return;
        if (Status != BookingStatus.Pending)
            throw new InvalidBookingTransitionException(BookingErrorCodes.ConfirmationNotAllowed,
                $"A {Status.ToString().ToLowerInvariant()} booking cannot be confirmed.");

        Status = BookingStatus.Confirmed;
        ConfirmedAt = now;
        PaymentDueAt = null;
        _domainEvents.Add(new BookingConfirmedEvent(Id, Code.Value, HotelId, now));
    }

    /// <summary>
    ///     Cancellation policy (US-07 scenario 4): only a Pending or Confirmed booking can be cancelled, and not on or
    ///     after its check-in day (hotel time). Its nights become free (R1). A guest can only cancel their own booking
    ///     (another's does not exist for them); staff only those of their hotel.
    /// </summary>
    public void Cancel(BookingRequester requester, DateTime hotelToday, DateTimeOffset now)
    {
        if (!IsVisibleTo(requester))
        {
            if (requester.IsGuest) throw new BookingNotFoundException(Id);
            throw new BookingOutsideHotelScopeException();
        }
        EnsureCancellable();
        if (hotelToday.Date >= CheckInDate.Date)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CancellationTooLate,
                $"A booking cannot be cancelled on or after its check-in day ({CheckInDate:yyyy-MM-dd}).");

        MarkCancelled(requester.IsGuest ? ValueObjects.CancellationReason.GuestRequest : ValueObjects.CancellationReason.HotelRequest, now);
    }

    /// <summary>
    ///     Payment hold: a Pending booking whose payment deadline passed is cancelled and frees the room.
    /// </summary>
    /// <returns>True when the booking expired now.</returns>
    public bool ExpireIfUnpaid(DateTimeOffset now)
    {
        if (Status != BookingStatus.Pending || PaymentDueAt is not { } due || now < due) return false;
        MarkCancelled(ValueObjects.CancellationReason.PaymentNotReceived, now);
        return true;
    }

    /// <summary>
    ///     US-07 scenario 3: moves the booking to other dates and/or another room of the same hotel. Availability of
    ///     the new stay is checked beforehand under the room row lock. A Pending booking takes the price of the new
    ///     room; a paid (Confirmed) booking can only change to a stay with the same total, because payments are not
    ///     adjusted (cancel and book again otherwise).
    /// </summary>
    public void Reschedule(BookingRequester requester, RoomOffer room, DateRange dates, DateTime hotelToday, DateTimeOffset now)
    {
        if (!requester.OperatesHotel(HotelId))
            throw new BookingOutsideHotelScopeException();
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            throw new InvalidBookingTransitionException(BookingErrorCodes.ChangeNotAllowed,
                $"A {Status.ToString().ToLowerInvariant()} booking cannot be changed.");
        if (room.HotelId != HotelId)
            throw new DomainValidationException(BookingErrorCodes.RoomOfOtherHotel, "A booking can only move to a room of the same hotel.");
        dates.EnsureNotInThePast(hotelToday);
        if (room.RoomId != RoomId) EnsureBookable(room);

        var newPrice = room.RoomId == RoomId ? PricePerNight : room.PricePerNight;
        var newTotal = decimal.Round(newPrice * dates.Nights, 2, MidpointRounding.AwayFromZero);
        if (Status == BookingStatus.Confirmed && newTotal != TotalPrice)
            throw new InvalidBookingTransitionException(BookingErrorCodes.PaidTotalMismatch,
                $"This booking is already paid ({TotalPrice:0.00}); it can only change to a stay with the same total (the new one costs {newTotal:0.00}). Cancel it and book again instead.");

        var previousRoom = RoomId;
        var previousCheckIn = CheckInDate;
        var previousCheckOut = CheckOutDate;
        if (previousRoom == room.RoomId && dates == Dates) return;

        RoomId = room.RoomId;
        PricePerNight = newPrice;
        CheckInDate = dates.CheckIn;
        CheckOutDate = dates.CheckOut;
        _domainEvents.Add(new BookingRescheduledEvent(Id, Code.Value, HotelId, previousRoom, previousCheckIn, previousCheckOut, now));
    }

    /// <summary>
    ///     US-08: the stay starts once the digital check-in is approved. Only a Confirmed (paid) booking, from its
    ///     check-in day until the day before its check-out (hotel time).
    /// </summary>
    public void CheckIn(DateTime hotelToday, DateTimeOffset now)
    {
        EnsureCheckInAllowed(hotelToday);
        Status = BookingStatus.CheckedIn;
        CheckedInAt = now;
        _domainEvents.Add(new GuestCheckedInEvent(Id, Code.Value, HotelId, RoomId, now));
    }

    /// <summary>
    ///     US-08 scenario 3: the guest has difficulties with the digital check-in and asks the front desk for help.
    ///     Only their own booking, while it is Pending or Confirmed and the stay has not ended.
    /// </summary>
    public void RequestCheckInAssistance(BookingRequester requester, string? message, DateTime hotelToday, DateTimeOffset now)
    {
        if (!IsOwnedBy(requester)) throw new BookingNotFoundException(Id);
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInAssistanceNotAllowed,
                $"Assistance with the check-in is only for pending or confirmed bookings; this one is {Status.ToString().ToLowerInvariant()}.");
        if (hotelToday.Date >= CheckOutDate.Date)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInStayEnded, "The stay of this booking has already ended.");

        var trimmed = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        if (trimmed is { Length: > 500 })
            throw new InvalidFieldException("message", BookingErrorCodes.CheckInMessageTooLong, "The message cannot exceed 500 characters.");
        _domainEvents.Add(new CheckInAssistanceRequestedEvent(Id, Code.Value, HotelId, RoomId, trimmed, now));
    }

    /// <summary>Throws when the digital check-in cannot be done today.</summary>
    public void EnsureCheckInAllowed(DateTime hotelToday)
    {
        if (Status == BookingStatus.CheckedIn)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInAlreadyCompleted, "The check-in of this booking is already completed.");
        if (Status != BookingStatus.Confirmed)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInBookingNotConfirmed,
                $"Only a confirmed (paid) booking can check in; this one is {Status.ToString().ToLowerInvariant()}.");
        if (hotelToday.Date < CheckInDate.Date)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInNotOpenYet,
                $"The digital check-in opens on the check-in day ({CheckInDate:yyyy-MM-dd}).");
        if (hotelToday.Date >= CheckOutDate.Date)
            throw new InvalidBookingTransitionException(BookingErrorCodes.CheckInStayEnded, "The stay of this booking has already ended.");
    }

    private void EnsureCancellable()
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            throw new InvalidBookingTransitionException(BookingErrorCodes.CancellationNotAllowed,
                $"Only pending or confirmed bookings can be cancelled; this one is {Status.ToString().ToLowerInvariant()}.");
    }

    private void MarkCancelled(CancellationReason reason, DateTimeOffset now)
    {
        var wasPaid = Status == BookingStatus.Confirmed;
        Status = BookingStatus.Cancelled;
        CancelledAt = now;
        CancellationReason = reason;
        PaymentDueAt = null;
        _domainEvents.Add(new BookingCancelledEvent(Id, Code.Value, HotelId, reason, wasPaid, now));
    }

    /// <summary>
    ///     US-53: without payment methods the guest could not pay, so a hotel does not take bookings until its
    ///     administrator sets them. Checked first (before availability) so the answer explains the real cause.
    /// </summary>
    /// <exception cref="HotelNotAcceptingBookingsException">The hotel has no payment methods.</exception>
    public static void EnsureHotelAcceptsBookings(RoomOffer room)
    {
        if (!room.HotelAcceptsBookings)
            throw new HotelNotAcceptingBookingsException(room.HotelId);
    }

    private static void EnsureBookable(RoomOffer room)
    {
        if (!room.IsOfferedForBooking)
            throw new RoomUnderMaintenanceException(room.RoomId);
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
    Completed = 3,
    CheckedIn = 4
}

public static class BookingStatusExtensions
{
    /// <summary>Active bookings hold the room (R1): Pending (until it expires), Confirmed and CheckedIn.</summary>
    public static bool IsActive(this BookingStatus status) =>
        status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn;
}
