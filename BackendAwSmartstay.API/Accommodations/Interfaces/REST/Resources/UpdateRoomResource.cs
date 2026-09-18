using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>Changes to a room (US-53). The hotel cannot change; a new price only applies to new bookings.</summary>
public record UpdateRoomResource
{
    /// <summary>Room type.</summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "Choose the room type.")]
    public int RoomTypeId { get; init; }

    /// <summary>Price per night, greater than 0.</summary>
    /// <example>130.00</example>
    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "The price per night must be between 0.01 and 100000.")]
    public decimal Price { get; init; }

    /// <summary>Description.</summary>
    [Required(ErrorMessage = "Enter a description of the room.")]
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>Amenities.</summary>
    public List<string>? Amenities { get; init; }

    /// <summary>New room number (optional), unique in the hotel.</summary>
    /// <example>103A</example>
    [MaxLength(10, ErrorMessage = "The room number can have at most 10 characters.")]
    public string? Number { get; init; }
}
