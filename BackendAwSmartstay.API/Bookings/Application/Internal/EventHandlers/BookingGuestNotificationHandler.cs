using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.EventHandlers;

/// <summary>
///     Keeps the guest informed of every change of their booking (US-51 scenario 2, US-07 scenarios 3 and 4, D1).
///     Runs after the commit, so an e-mail never announces a change that was rolled back.
/// </summary>
public class BookingGuestNotificationHandler(
    IBookingRepository bookingRepository,
    IAccommodationsContextFacade accommodationsContextFacade,
    IPaymentsContextFacade paymentsContextFacade,
    IBookingNotificationService notifications) :
    IDomainEventHandler<BookingCreatedEvent>,
    IDomainEventHandler<BookingConfirmedEvent>,
    IDomainEventHandler<BookingCancelledEvent>,
    IDomainEventHandler<BookingRescheduledEvent>
{
    public async Task HandleAsync(BookingCreatedEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        await notifications.SendBookingPlacedAsync(booking, await accommodationsContextFacade.FetchHotelAsync(e.HotelId),
            paymentsContextFacade.GetPaymentInstructions());
    }

    public async Task HandleAsync(BookingConfirmedEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        await notifications.SendBookingConfirmedAsync(booking, await accommodationsContextFacade.FetchHotelAsync(e.HotelId));
    }

    public async Task HandleAsync(BookingCancelledEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        await notifications.SendBookingCancelledAsync(booking, await accommodationsContextFacade.FetchHotelAsync(e.HotelId));
    }

    public async Task HandleAsync(BookingRescheduledEvent e, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindByIdAsync(e.BookingId);
        if (booking is null) return;
        await notifications.SendBookingRescheduledAsync(booking, await accommodationsContextFacade.FetchHotelAsync(e.HotelId),
            e.PreviousCheckIn, e.PreviousCheckOut, e.PreviousRoomId);
    }
}
