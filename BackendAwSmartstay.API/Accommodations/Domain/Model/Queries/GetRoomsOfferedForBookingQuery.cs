namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

/// <summary>Rooms that can be offered for booking (not under maintenance), of one hotel or of every hotel.</summary>
/// <param name="HotelId">Only this hotel, or null for every hotel.</param>
public record GetRoomsOfferedForBookingQuery(int? HotelId);
