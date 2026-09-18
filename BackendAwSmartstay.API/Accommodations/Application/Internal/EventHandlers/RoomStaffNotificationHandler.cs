using BackendAwSmartstay.API.Accommodations.Application.OutboundServices;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Events;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.EventHandlers;

/// <summary>
///     Notifies the hotel staff in charge of a room's new status (US-06 scenario 1) and the hotel administrators of an
///     overdue maintenance (scenario 4). Recipients come from IAM through its ACL facade.
/// </summary>
/// <remarks>
///     Who is in charge of each status: <b>Cleaning</b> and <b>Occupied</b> → housekeeping (clean it / do not disturb,
///     service the stay); <b>Maintenance</b> → maintenance and the hotel admin; <b>Available</b> → reception (the room
///     can be sold and assigned). The staff member who made the change is not notified. Changes made by a check-in
///     are announced by the Bookings context (check-in e-mail to housekeeping), not here.
/// </remarks>
public class RoomStaffNotificationHandler(
    IRoomRepository roomRepository,
    IHotelRepository hotelRepository,
    IIamContextFacade iamContextFacade,
    IRoomNotificationService notifications) :
    IDomainEventHandler<RoomStatusChangedEvent>,
    IDomainEventHandler<RoomMaintenanceOverdueEvent>
{
    public static IReadOnlyList<string> RolesInChargeOf(RoomStatus status) => status switch
    {
        RoomStatus.Cleaning or RoomStatus.Occupied => [UserRoles.Housekeeping],
        RoomStatus.Maintenance => [UserRoles.Maintenance, UserRoles.Admin],
        _ => [UserRoles.Reception]
    };

    public async Task HandleAsync(RoomStatusChangedEvent e, CancellationToken cancellationToken)
    {
        if (e.Origin != RoomStatusChangeOrigin.Staff) return;

        var recipients = (await iamContextFacade.ListHotelStaffAsync(e.HotelId, RolesInChargeOf(e.ToStatus).ToList()))
            .Where(contact => contact.UserId != e.ChangedByUserId).ToList();
        if (recipients.Count == 0) return;

        var changedBy = e.ChangedByUserId is { } actorId ? (await iamContextFacade.FetchUserContactAsync(actorId)) : null;
        await notifications.SendStatusChangedAsync(recipients, await NoticeAsync(e.RoomId, e.HotelId), e.FromStatus, e.ToStatus,
            changedBy?.FullName ?? changedBy?.Email);
    }

    public async Task HandleAsync(RoomMaintenanceOverdueEvent e, CancellationToken cancellationToken)
    {
        var recipients = await iamContextFacade.ListHotelStaffAsync(e.HotelId, [UserRoles.Admin]);
        // A hotel without an administrator: the chain administrators are alerted instead.
        if (recipients.Count == 0)
            recipients = await iamContextFacade.ListHotelStaffAsync(e.HotelId, [UserRoles.ChainAdmin]);
        if (recipients.Count == 0) return;

        await notifications.SendMaintenanceOverdueAsync(recipients, await NoticeAsync(e.RoomId, e.HotelId), e.MaintenanceSince);
    }

    private async Task<RoomNotice> NoticeAsync(int roomId, int hotelId)
    {
        var room = await roomRepository.FindByIdAsync(roomId);
        var hotel = await hotelRepository.FindByIdAsync(hotelId);
        return new RoomNotice(roomId, room?.Number ?? roomId.ToString(), hotelId, hotel?.Name ?? $"Hotel {hotelId}");
    }
}
