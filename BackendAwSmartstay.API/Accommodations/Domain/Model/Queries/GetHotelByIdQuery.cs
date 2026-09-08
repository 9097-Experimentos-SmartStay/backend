namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

/// <summary>
/// Query to retrieve all rooms for a specific hotel.
/// </summary>
/// <param name="HotelId"></param>
public record GetHotelByIdQuery(int HotelId);