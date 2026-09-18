using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>A new room of a hotel (US-53).</summary>
public record CreateRoomResource
{
    /// <summary>The hotel (an admin: their own hotel).</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Choose the hotel of the room.")]
    public int HotelId { get; init; }

    /// <summary>Room number, unique in the hotel: 1 to 10 letters, digits or hyphens.</summary>
    /// <example>103</example>
    [Required(ErrorMessage = "Enter the room number.")]
    [MaxLength(10, ErrorMessage = "The room number can have at most 10 characters.")]
    public string? Number { get; init; }

    /// <summary>Room type (from <c>GET /room-types</c>).</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Choose the room type.")]
    public int RoomTypeId { get; init; }

    /// <summary>Price per night, greater than 0.</summary>
    /// <example>120.00</example>
    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "The price per night must be between 0.01 and 100000.")]
    public decimal Price { get; init; }

    /// <summary>Description.</summary>
    /// <example>Room 103 - Garden view.</example>
    [Required(ErrorMessage = "Enter a description of the room.")]
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>Amenities.</summary>
    public List<string>? Amenities { get; init; }
}
