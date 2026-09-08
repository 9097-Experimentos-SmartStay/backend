using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert UpdateHotelResource to UpdateHotelCommand.
/// </summary>
public static class UpdateHotelCommandFromResourceAssembler
{
    public static UpdateHotelCommand ToCommandFromResource(int hotelId, UpdateHotelResource resource)
    {
        return new UpdateHotelCommand(
            hotelId,
            resource.Name,
            resource.Address,
            resource.City,
            resource.Country,
            resource.ImageUrl,
            resource.Description,
            resource.Type,
            resource.Amenities
        );
    }
}