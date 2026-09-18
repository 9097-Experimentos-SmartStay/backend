using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Application.OutboundServices;

/// <summary>The room and hotel an e-mail is about.</summary>
public sealed record RoomNotice(int RoomId, string RoomDescription, int HotelId, string HotelName);

/// <summary>E-mails to the hotel staff about the rooms (US-06). Called after the commit.</summary>
public interface IRoomNotificationService
{
    /// <summary>Scenario 1: the status of a room changed; the staff in charge of the new status is notified.</summary>
    Task SendStatusChangedAsync(IReadOnlyList<UserContact> recipients, RoomNotice room, RoomStatus from, RoomStatus to, string? changedBy);

    /// <summary>Scenario 4: the room has been under maintenance for too long.</summary>
    Task SendMaintenanceOverdueAsync(IReadOnlyList<UserContact> recipients, RoomNotice room, DateTimeOffset maintenanceSince);
}
