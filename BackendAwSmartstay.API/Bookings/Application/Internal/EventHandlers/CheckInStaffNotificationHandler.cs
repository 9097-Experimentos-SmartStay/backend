using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.EventHandlers;

/// <summary>
///     US-08: a completed check-in notifies the housekeeping staff of the hotel (scenario 4) and a request for help
///     notifies the front desk (scenario 3; the hotel admins when the hotel has no reception user).
/// </summary>
public class CheckInStaffNotificationHandler(
    IBookingRepository bookingRepository,
    IAccommodationsContextFacade accommodationsContextFacade,
    IIamContextFacade iamContextFacade,
    ICheckInNotificationService notifications) :
    IDomainEventHandler<GuestCheckedInEvent>,
    IDomainEventHandler<CheckInAssistanceRequestedEvent>
{
    public async Task HandleAsync(GuestCheckedInEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        var housekeeping = await iamContextFacade.ListHotelStaffAsync(e.HotelId, [UserRoles.Housekeeping]);
        await notifications.SendGuestCheckedInAsync(housekeeping, booking, await accommodationsContextFacade.FetchHotelAsync(e.HotelId));
    }

    public async Task HandleAsync(CheckInAssistanceRequestedEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        var frontDesk = await iamContextFacade.ListHotelStaffAsync(e.HotelId, [UserRoles.Reception]);
        if (frontDesk.Count == 0)
            frontDesk = await iamContextFacade.ListHotelStaffAsync(e.HotelId, [UserRoles.Admin]);
        await notifications.SendCheckInAssistanceRequestedAsync(frontDesk, booking,
            await accommodationsContextFacade.FetchHotelAsync(e.HotelId), e.Message);
    }
}
