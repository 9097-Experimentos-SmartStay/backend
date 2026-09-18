using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class RoomMapResourceAssembler
{
    public static RoomMapResource ToResource(Hotel hotel, IReadOnlyList<Room> rooms, DateTimeOffset now, TimeSpan maintenanceThreshold) =>
        new(hotel.Id, hotel.Name, now,
            Enum.GetValues<RoomStatus>().ToDictionary(status => status.ToString(), status => rooms.Count(room => room.Status == status)),
            rooms.Select(room => new RoomMapItemResource(room.Id, room.RoomType?.Name ?? string.Empty, room.Description,
                room.Price, room.Status.ToString(), room.StatusChangedAt,
                room.Status == RoomStatus.Maintenance && now - room.StatusChangedAt >= maintenanceThreshold,
                RoomStatusTransitions.AllowedFrom(room.Status).Select(status => status.ToString()).ToList())).ToList());

    public static RoomStatusChangeResource ToResource(RoomStatusChange change) =>
        new(change.Id, change.RoomId, change.FromStatus.ToString(), change.ToStatus.ToString(), change.Origin.ToString(),
            change.ChangedAt, change.ChangedByUserId, change.ChangedByEmail);
}
