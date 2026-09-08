using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;


public static class RoomTypeResourceFromEntityAssembler
{

    public static RoomTypeResource ToResourceFromEntity(RoomType entity)
    {
        return new RoomTypeResource(entity.Id, entity.Name, entity.Description);
    }
}

