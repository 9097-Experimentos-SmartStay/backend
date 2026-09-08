using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

/// <summary>
///     Represents a booking aggregate in the domain, encapsulating booking-related business logic.
/// </summary>
public class Booking
{
    public Booking()
    {
        GuestName = string.Empty;
        GuestEmail = string.Empty;
    }

    public Booking(int roomId, string guestName, string guestEmail, DateTime checkInDate,
        DateTime checkOutDate) : this()
    {
        RoomId = roomId;
        GuestName = guestName;
        GuestEmail = guestEmail;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Status = BookingStatus.Pending;
    }

    public Booking(CreateBookingCommand command) : this(
        command.RoomId,
        command.GuestName,
        command.GuestEmail,
        command.CheckInDate,
        command.CheckOutDate)
    {
    }

    /// <summary>
    ///     The unique identifier of the booking.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     The identifier of the room being booked.
    /// </summary>
    public int RoomId { get; private set; }

    /// <summary>
    ///     The name of the guest making the booking.
    /// </summary>
    public string GuestName { get; private set; }

    /// <summary>
    ///     The email address of the guest.
    /// </summary>
    public string GuestEmail { get; private set; }

    /// <summary>
    ///     The check-in date of the booking.
    /// </summary>
    public DateTime CheckInDate { get; private set; }

    /// <summary>
    ///     The check-out date of the booking.
    /// </summary>
    public DateTime CheckOutDate { get; private set; }

    /// <summary>
    ///     The current status of the booking.
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    ///     Confirms the booking by changing its status to Confirmed.
    /// </summary>
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

    /// <summary>
    ///     Cancels the booking by changing its status to Cancelled.
    /// </summary>
    public void Cancel()
    {
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