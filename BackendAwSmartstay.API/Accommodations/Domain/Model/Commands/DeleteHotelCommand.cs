namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to delete an existing hotel aggregate.
/// </summary>
/// <param name="Id">The unique identifier of the hotel to delete.</param>
public record DeleteHotelCommand(int Id);