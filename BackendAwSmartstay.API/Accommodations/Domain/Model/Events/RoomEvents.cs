using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Events;

/// <summary>The status of a room changed (US-06 scenario 1).</summary>
public sealed record RoomStatusChangedEvent(int RoomId, int HotelId, RoomStatus FromStatus, RoomStatus ToStatus,
    RoomStatusChangeOrigin Origin, int? ChangedByUserId, DateTimeOffset OccurredOn) : DomainEvent(OccurredOn);

/// <summary>A room has been under maintenance longer than allowed (US-06 scenario 4).</summary>
public sealed record RoomMaintenanceOverdueEvent(int RoomId, int HotelId, DateTimeOffset MaintenanceSince, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);
