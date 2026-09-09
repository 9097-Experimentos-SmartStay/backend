using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;

/// <summary>
/// Service implementation for handling booking commands.
/// Orchestrates the flow between the repository, domain logic, and external ACLs.
/// </summary>
public class BookingCommandService(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IGuestProfilesContextFacade guestProfilesContextFacade)
    : IBookingCommandService
{
    /// <summary>
    /// Handles the creation of a new booking, resolving associated GuestProfileId via ACL if available.
    /// </summary>
    /// <param name="command">The command containing the booking creation data.</param>
    /// <returns>The created booking or null if creation failed.</returns>
    public async Task<Booking?> Handle(CreateBookingCommand command)
    {
        Guid? guestProfileId = command.GuestProfileId;

        if (!guestProfileId.HasValue)
        {
            if (command.UserId.HasValue && command.UserId.Value > 0)
            {
                guestProfileId = await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(command.UserId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(command.GuestEmail))
            {
                guestProfileId = await guestProfilesContextFacade.FetchGuestProfileIdByEmailAsync(command.GuestEmail);
            }
        }

        var booking = new Booking(command, guestProfileId);
        await bookingRepository.AddAsync(booking);
        await unitOfWork.CompleteAsync();

        return booking;
    }

    /// <summary>
    /// Handles the confirmation of an existing booking.
    /// </summary>
    /// <param name="command">The command containing the booking confirmation data.</param>
    /// <returns>The confirmed booking or null if the booking was not found.</returns>
    public async Task<Booking?> Handle(ConfirmBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId);
        if (booking is null) return null;

        booking.Confirm();
        bookingRepository.Update(booking);
        await unitOfWork.CompleteAsync();

        return booking;
    }

    /// <summary>
    /// Handles the cancellation of an existing booking.
    /// </summary>
    /// <param name="command">The command containing the booking cancellation data.</param>
    /// <returns>The cancelled booking or null if the booking was not found.</returns>
    public async Task<Booking?> Handle(CancelBookingCommand command)
    {
        var booking = await bookingRepository.FindByIdAsync(command.BookingId);
        if (booking is null) return null;

        booking.Cancel();
        bookingRepository.Update(booking);
        await unitOfWork.CompleteAsync();

        return booking;
    }
}
