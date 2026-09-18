using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
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
        DateTime checkOutDate, Guid? guestProfileId = null, int? userId = null) : this()
    {
        if (roomId <= 0)
            throw new DomainValidationException("A booking must reference a valid room.");
        if (checkOutDate.Date <= checkInDate.Date)
            throw new DomainValidationException("The check-out date must be at least one day after the check-in date.");

        RoomId = roomId;
        UserId = userId;
        GuestName = guestName;
        GuestEmail = guestEmail;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        GuestProfileId = guestProfileId;
        Status = BookingStatus.Pending;
    }

    public Booking(CreateBookingCommand command, Guid? guestProfileId = null) : this(
        command.RoomId,
        command.GuestName,
        command.GuestEmail,
        command.CheckInDate,
        command.CheckOutDate,
        guestProfileId ?? command.GuestProfileId,
        command.UserId)
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
    ///     The logical external identifier of the associated guest profile.
    /// </summary>
    public Guid? GuestProfileId { get; private set; }

    /// <summary>
    ///     The IAM user that owns the booking (the guest account that created it), if any.
    /// </summary>
    public int? UserId { get; private set; }

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
    ///     Number of nights covered by the booking (check-out date minus check-in date).
    /// </summary>
    public int Nights => (CheckOutDate.Date - CheckInDate.Date).Days;

    /// <summary>
    ///     A booking can be paid while it is Pending or Confirmed.
    /// </summary>
    public bool CanBePaid => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    /// <summary>
    ///     Ownership rule: a guest owns a booking when it was created with their user id,
    ///     or when it is attached to their guest profile.
    /// </summary>
    /// <param name="userId">The IAM user id of the guest.</param>
    /// <param name="guestProfileId">The guest profile linked to that user, if any.</param>
    public bool IsOwnedBy(int userId, Guid? guestProfileId)
    {
        if (UserId.HasValue && UserId.Value == userId) return true;
        return guestProfileId.HasValue && GuestProfileId.HasValue && GuestProfileId.Value == guestProfileId.Value;
    }

    /// <summary>
    ///     Confirms the booking by changing its status to Confirmed. Confirming twice is a no-op.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the booking is cancelled or completed.</exception>
    public void Confirm()
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.Completed)
            throw new BusinessRuleViolationException($"A {Status.ToString().ToLowerInvariant()} booking cannot be confirmed.");

        Status = BookingStatus.Confirmed;
    }

    /// <summary>
    ///     Cancels the booking by changing its status to Cancelled. Cancelling twice is a no-op.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the booking is already completed.</exception>
    public void Cancel()
    {
        if (Status == BookingStatus.Completed)
            throw new BusinessRuleViolationException("A completed booking cannot be cancelled.");

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