using BackendAwSmartstay.API.Bookings.Domain.Repositories; // <--- IMPORTANTE: Acceso a Reservas
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;

/// <summary>
/// Implementation of the payment command service.
/// Orchestrates the payment process and updates the booking status upon success.
/// </summary>
public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    IBookingRepository bookingRepository, // <--- Inject the reserve repository
    IUnitOfWork unitOfWork) 
    : IPaymentCommandService
{
    /// <summary>
    /// Processes a payment command, simulating bank validation and updating the associated booking if successful.
    /// </summary>
    /// <param name="command">The command containing the payment data.</param>
    /// <returns>The processed payment or null if processing failed.</returns>
    public async Task<Payment?> Handle(ProcessPaymentCommand command)
    {
        // 1. Start the payment process (Pending Status)
        var payment = new Payment(command);

        // 2. Simulation Logic (Fake Gateway)
        bool isApproved = true;

        // Simulated business rules
        if (command.Amount <= 0) isApproved = false;
        if (command.CardNumber.EndsWith("0000")) isApproved = false; // Simulate declined card

        if (isApproved)
        {
            payment.Complete(); // Change payment status to Completed (1)

            // --- KEY PLAY: UPDATE RESERVATION ---
            var booking = await bookingRepository.FindByIdAsync(command.BookingId);
            if (booking != null)
            {
                booking.Confirm(); // Change reservation status to Confirmed
                bookingRepository.Update(booking);
                Console.WriteLine($"Booking #{booking.Id} has been confirmed via Payment.");
            }
            else 
            {
                // If there is no reservation, we shouldn't charge.
                throw new Exception("Booking not found provided for payment.");
            }
            // -------------------------------------------
        }
        else
        {
            payment.Fail(); // Change payment status to Failed (2)
            Console.WriteLine("Payment declined by simulation logic.");
        }

        // 3. Save EVERYTHING in a single transaction (Unit of Work)

        // This saves the payment and the reservation update at the same time.
        await paymentRepository.AddAsync(payment);
        await unitOfWork.CompleteAsync();

        return payment;
    }
}