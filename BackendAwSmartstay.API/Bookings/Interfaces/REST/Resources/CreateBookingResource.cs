using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>
///     A new booking. A guest books for themselves (their account gives the name and e-mail). Staff book for a guest
///     identified by <c>userId</c> (an account) or by <c>guestName</c> + <c>guestEmail</c> (a phone or walk-in guest
///     without an account).
/// </summary>
public record CreateBookingResource
{
    /// <summary>The room to book (from <c>GET /rooms/available</c>).</summary>
    /// <example>101</example>
    [Range(1, int.MaxValue, ErrorMessage = "Choose a room.")]
    public int RoomId { get; init; }

    /// <summary>Check-in date (yyyy-MM-dd): today or later, hotel time.</summary>
    /// <example>2026-10-01</example>
    [Required]
    public DateTime? CheckInDate { get; init; }

    /// <summary>Check-out date (yyyy-MM-dd): at least one day after the check-in.</summary>
    /// <example>2026-10-04</example>
    [Required]
    public DateTime? CheckOutDate { get; init; }

    /// <summary>Guest name (required for staff bookings without <c>userId</c>).</summary>
    /// <example>Ana Pérez</example>
    [MaxLength(100)]
    public string? GuestName { get; init; }

    /// <summary>Guest e-mail (required for staff bookings without <c>userId</c>): the booking e-mails go there.</summary>
    /// <example>ana.perez@example.com</example>
    [MaxLength(200)]
    public string? GuestEmail { get; init; }

    /// <summary>Guest phone, optional (7 to 15 digits, may start with +).</summary>
    /// <example>+51987654321</example>
    [MaxLength(25)]
    public string? GuestPhone { get; init; }

    /// <summary>Staff only: the guest account to book for.</summary>
    public int? UserId { get; init; }

    /// <summary>Staff only: the guest profile to attach.</summary>
    public Guid? GuestProfileId { get; init; }
}
