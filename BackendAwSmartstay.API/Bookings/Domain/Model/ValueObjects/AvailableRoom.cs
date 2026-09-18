using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>A room free for a stay, with the price of that stay.</summary>
/// <param name="Room">The room as offered by the Accommodations context.</param>
/// <param name="Dates">The stay.</param>
public sealed record AvailableRoom(RoomOffer Room, DateRange Dates)
{
    public int Nights => Dates.Nights;

    /// <summary>Price per night × nights.</summary>
    public decimal TotalPrice => Room.PricePerNight * Nights;
}
