using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class RoomResourceFromEntityAssembler
{
    public static RoomResource ToResourceFromEntity(Room entity)
    {
        return new RoomResource(
            entity.Id,
            entity.HotelId,
            entity.RoomTypeId,
            entity.RoomType?.Name ?? "Unknown", // Safe navigation
            entity.Price,
            entity.Description,
            entity.Amenities
        );
    }
}