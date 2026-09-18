using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;

/// <summary>
/// Implementation of the payment command service.
/// Orchestrates the payment process and confirms the booking upon success.
/// </summary>
/// <remarks>
///     Payments never touches the Bookings or Accommodations repositories: it reads the booking and the room
///     price through their ACL facades and confirms the booking through the Bookings application layer.
/// </remarks>
public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    IBookingsContextFacade bookingsContextFacade,
    IAccommodationsContextFacade accommodationsContextFacade,
    IUnitOfWork unitOfWork,
    ILogger<PaymentCommandService> logger) 
    : IPaymentCommandService
{
    /// <summary>
    /// Processes a payment command, simulating bank validation and confirming the associated booking if successful.
    /// </summary>
    /// <param name="command">The command containing the payment data.</param>
    /// <returns>The processed payment (Completed or Failed).</returns>
    /// <exception cref="KeyNotFoundException">The booking does not exist, or a guest does not own it (404).</exception>
    /// <exception cref="InvalidOperationException">The booking cannot be paid or is already paid (409).</exception>
    public async Task<Payment?> Handle(ProcessPaymentCommand command)
    {
        // 1. Load the booking through the Bookings ACL (ownership enforced for guests)
        var booking = await bookingsContextFacade.FetchBookingAsync(command.BookingId, command.GuestUserId)
                      ?? throw new EntityNotFoundException("Booking", command.BookingId);

        if (!booking.CanBePaid)
            throw new BusinessRuleViolationException(
                $"Booking {booking.BookingId} is {booking.Status.ToLowerInvariant()} and cannot be paid.");

        if (await paymentRepository.ExistsCompletedForBookingAsync(booking.BookingId))
            throw new BusinessRuleViolationException($"Booking {booking.BookingId} is already paid.");

        // 2. The amount is computed by the backend: room price per night × nights
        var pricePerNight = await accommodationsContextFacade.FetchRoomPricePerNightAsync(booking.RoomId)
                            ?? throw new BusinessRuleViolationException(
                                $"Room {booking.RoomId} of booking {booking.BookingId} no longer exists.");
        var amount = PaymentAmountCalculator.Calculate(pricePerNight, booking.CheckInDate, booking.CheckOutDate);

        var payment = new Payment(command, amount);

        // 3. Simulation Logic (Fake Gateway)
        var isApproved = amount > 0 && !command.CardNumber.EndsWith("0000"); // "0000" simulates a declined card

        await paymentRepository.AddAsync(payment);

        if (!isApproved)
        {
            payment.Fail();
            logger.LogInformation("Payment for booking {BookingId} declined by simulation logic.", booking.BookingId);
            await unitOfWork.CompleteAsync();
            return payment;
        }

        payment.Complete();

        // 4. Confirm the booking through the Bookings application layer. It commits the shared unit of work,
        //    so the payment and the booking confirmation are saved in the same SaveChanges.
        if (!await bookingsContextFacade.ConfirmBookingAsync(booking.BookingId))
            throw new EntityNotFoundException("Booking", booking.BookingId);

        logger.LogInformation("Booking {BookingId} confirmed via payment {TransactionId} ({Amount}).",
            booking.BookingId, payment.TransactionId, amount);

        return payment;
    }
}
