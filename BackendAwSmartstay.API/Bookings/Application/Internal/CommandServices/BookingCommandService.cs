using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;

/// <summary>
/// Orchestrates booking commands: resolves data from other contexts through their ACL facades, delegates every
/// rule to the domain (Booking aggregate, RoomAvailabilityService) and commits the unit of work.
/// </summary>
public class BookingCommandService(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IGuestProfilesContextFacade guestProfilesContextFacade,
    IAccommodationsContextFacade accommodationsContextFacade,
    RoomAvailabilityService roomAvailabilityService)
    : IBookingCommandService
{
    public async Task<Booking> Handle(CreateBookingCommand command)
    {
        var dates = new DateRange(command.CheckInDate, command.CheckOutDate);
        var requester = await ResolveGuestProfileAsync(command.Requester);
        var guestProfileId = requester.IsGuest
            ? requester.GuestProfileId
            : command.GuestProfileId ?? await FindGuestProfileForStaffBookingAsync(command);

        Booking? booking = null;
        // R1 under concurrency: the room row is locked for the transaction, so two requests for the same room
        // run the availability check and the insert one after the other instead of both seeing a free room.
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (!await accommodationsContextFacade.LockRoomForBookingAsync(command.RoomId))
                throw new DomainValidationException($"Room {command.RoomId} does not exist.");

            await roomAvailabilityService.EnsureRoomIsAvailableAsync(command.RoomId, dates);

            booking = Booking.Create(requester, command.RoomId, dates, command.GuestName, command.GuestEmail,
                command.UserId, guestProfileId);
            await bookingRepository.AddAsync(booking);
            await unitOfWork.CompleteAsync();
        });
        return booking!;
    }

    public async Task<Booking> Handle(ConfirmBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId)
                      ?? throw new BookingNotFoundException(command.BookingId);

        booking.Confirm();
        bookingRepository.Update(booking);
        await unitOfWork.CompleteAsync();
        return booking;
    }

    public async Task<Booking> Handle(CancelBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId)
                      ?? throw new BookingNotFoundException(command.BookingId);

        booking.Cancel(await ResolveGuestProfileAsync(command.Requester));
        bookingRepository.Update(booking);
        await unitOfWork.CompleteAsync();
        return booking;
    }

    /// <summary>A guest's ownership also covers bookings attached to their guest profile (Profiles ACL).</summary>
    private async Task<BookingRequester> ResolveGuestProfileAsync(BookingRequester requester) =>
        requester.IsGuest
            ? requester.WithGuestProfile(await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(requester.UserId))
            : requester;

    /// <summary>Desk bookings are attached to the guest's profile when it can be found by account or e-mail.</summary>
    private async Task<Guid?> FindGuestProfileForStaffBookingAsync(CreateBookingCommand command)
    {
        if (command.UserId is > 0)
            return await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(command.UserId.Value);
        if (!string.IsNullOrWhiteSpace(command.GuestEmail))
            return await guestProfilesContextFacade.FetchGuestProfileIdByEmailAsync(command.GuestEmail);
        return null;
    }
}
