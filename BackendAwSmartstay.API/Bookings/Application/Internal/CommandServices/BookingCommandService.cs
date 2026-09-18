using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;

/// <summary>
/// Orchestrates booking commands: resolves data from other contexts through their ACL facades, delegates every
/// rule to the domain (Booking aggregate, RoomAvailabilityService) and commits the unit of work. The e-mails are
/// enlisted in the outbox by the handlers of the booking events, in the same transaction as the changes.
/// </summary>
public class BookingCommandService(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IGuestProfilesContextFacade guestProfilesContextFacade,
    IAccommodationsContextFacade accommodationsContextFacade,
    IIamContextFacade iamContextFacade,
    RoomAvailabilityService roomAvailabilityService,
    HotelCalendar calendar,
    IOptions<BookingPolicySettings> settings,
    ILogger<BookingCommandService> logger)
    : IBookingCommandService
{
    public async Task<Booking> Handle(CreateBookingCommand command)
    {
        var dates = new DateRange(command.CheckInDate, command.CheckOutDate);
        var requester = await ResolveGuestProfileAsync(command.Requester);
        var contact = await ResolveContactAsync(command, requester);
        var guestProfileId = requester.IsGuest
            ? requester.GuestProfileId
            : command.GuestProfileId ?? await FindGuestProfileForStaffBookingAsync(command, contact);

        Booking? booking = null;
        // R1 under concurrency: the room row is locked for the transaction, so two requests for the same room
        // run the availability check and the insert one after the other instead of both seeing a free room.
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var room = await accommodationsContextFacade.LockRoomForBookingAsync(command.RoomId)
                       ?? throw new InvalidFieldException("roomId", BookingErrorCodes.RoomNotFound, $"Room {command.RoomId} does not exist.");
            Booking.EnsureHotelAcceptsBookings(room);

            await roomAvailabilityService.EnsureRoomIsAvailableAsync(room.RoomId, dates);

            booking = Booking.Place(requester, room, dates, contact, command.UserId, guestProfileId,
                calendar.Today, calendar.Now, settings.Value.PaymentHold);
            await bookingRepository.AddAsync(booking);
            await unitOfWork.CompleteAsync();
        });
        return booking!;
    }

    public async Task<Booking> Handle(ConfirmBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId)
                      ?? throw new BookingNotFoundException(command.BookingId);

        booking.Confirm(calendar.Now);
        await unitOfWork.CompleteAsync();
        return booking;
    }

    public async Task<Booking> Handle(CancelBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId)
                      ?? throw new BookingNotFoundException(command.BookingId);

        booking.Cancel(await ResolveGuestProfileAsync(command.Requester), calendar.Today, calendar.Now);
        await unitOfWork.CompleteAsync();
        return booking;
    }

    public async Task<Booking> Handle(RescheduleBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId)
                      ?? throw new BookingNotFoundException(command.BookingId);
        if (!booking.IsVisibleTo(command.Requester))
            throw new BookingNotFoundException(command.BookingId);

        var dates = new DateRange(command.CheckInDate ?? booking.CheckInDate, command.CheckOutDate ?? booking.CheckOutDate);
        var roomId = command.RoomId ?? booking.RoomId;

        // Same row lock as a new booking: the new stay is checked and saved while no other booking of that room
        // can be created or moved.
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var room = await accommodationsContextFacade.LockRoomForBookingAsync(roomId)
                       ?? throw new InvalidFieldException("roomId", BookingErrorCodes.RoomNotFound, $"Room {roomId} does not exist.");
            await roomAvailabilityService.EnsureRoomIsAvailableAsync(room.RoomId, dates, excludingBookingId: booking.Id);

            booking.Reschedule(command.Requester, room, dates, calendar.Today, calendar.Now);
            await unitOfWork.CompleteAsync();
        });
        return booking;
    }

    public async Task<int> Handle(ExpireUnpaidBookingsCommand command)
    {
        var now = calendar.Now;
        var expired = 0;
        foreach (var booking in await bookingRepository.ListPendingPaymentDueAsync(now))
            if (booking.ExpireIfUnpaid(now)) expired++;

        await unitOfWork.CompleteAsync();
        if (expired > 0) logger.LogInformation("{Count} unpaid bookings expired and released their rooms.", expired);
        return expired;
    }

    /// <summary>
    ///     Who is staying and where the booking e-mails go. A guest: their account (name from the request if given).
    ///     Staff: the account they book for, or the name and e-mail typed for a guest without an account.
    /// </summary>
    private async Task<GuestContact> ResolveContactAsync(CreateBookingCommand command, BookingRequester requester)
    {
        var accountId = requester.IsGuest ? requester.UserId : command.UserId;
        if (accountId is > 0)
        {
            var account = await iamContextFacade.FetchUserContactAsync(accountId.Value);
            if (account is null && !requester.IsGuest)
                throw new InvalidFieldException("userId", BookingErrorCodes.GuestAccountInvalid, $"User {accountId} does not exist or is inactive.");
            if (account is not null)
                return new GuestContact(
                    string.IsNullOrWhiteSpace(command.GuestName) ? account.FullName ?? account.Email : command.GuestName,
                    // A guest's booking e-mails always go to their own account.
                    requester.IsGuest || string.IsNullOrWhiteSpace(command.GuestEmail) ? account.Email : command.GuestEmail,
                    command.GuestPhone);
        }
        return new GuestContact(command.GuestName, command.GuestEmail, command.GuestPhone);
    }

    /// <summary>A guest's ownership also covers bookings attached to their guest profile (Profiles ACL).</summary>
    private async Task<BookingRequester> ResolveGuestProfileAsync(BookingRequester requester) =>
        requester.IsGuest
            ? requester.WithGuestProfile(await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(requester.UserId))
            : requester;

    /// <summary>Desk bookings are attached to the guest's profile when it can be found by account or e-mail.</summary>
    private async Task<Guid?> FindGuestProfileForStaffBookingAsync(CreateBookingCommand command, GuestContact contact)
    {
        if (command.UserId is > 0)
            return await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(command.UserId.Value);
        return await guestProfilesContextFacade.FetchGuestProfileIdByEmailAsync(contact.Email);
    }
}
