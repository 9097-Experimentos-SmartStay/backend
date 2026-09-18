using BackendAwSmartstay.API.Payments.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;

/// <summary>
///     Registers booking payments through the <see cref="IPaymentGateway"/> port and confirms the booking (D1).
/// </summary>
/// <remarks>
///     Payments never touches the Bookings repositories: it reads the booking through its ACL facade and confirms it
///     through the Bookings application layer <b>in the same transaction</b>, so a payment is never saved without its
///     booking being confirmed (nor the other way round). The <c>PaymentCompletedEvent</c> is still published after the
///     commit for any other subscriber.
/// </remarks>
public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    IBookingsContextFacade bookingsContextFacade,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<PaymentCommandService> logger)
    : IPaymentCommandService
{
    public async Task<Payment> Handle(RegisterPaymentCommand command)
    {
        Payment? payment = null;
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var booking = await bookingsContextFacade.FetchBookingAsync(command.BookingId)
                          ?? throw new EntityNotFoundException("Booking", command.BookingId);
            if (!command.AllHotels && command.StaffHotelId != booking.HotelId)
                throw new OperationNotAllowedException(PaymentErrorCodes.OutsideHotelScope, "You can only register payments of the bookings of your hotel.");
            if (await paymentRepository.ExistsCompletedForBookingAsync(booking.BookingId))
                throw new BusinessRuleViolationException(PaymentErrorCodes.BookingAlreadyPaid, $"Booking {booking.Code} is already paid.");
            if (!booking.CanBePaid)
                throw new BusinessRuleViolationException(PaymentErrorCodes.BookingNotPending,
                    $"Booking {booking.Code} is {booking.Status.ToLowerInvariant()}: only a pending booking can be paid.");

            var now = timeProvider.GetUtcNow();
            payment = Payment.Register(booking.BookingId, booking.TotalPrice, command.Method, command.OperationNumber,
                command.Note, command.StaffUserId, now);

            var result = await paymentGateway.ChargeAsync(new PaymentCharge(booking.BookingId, booking.Code,
                payment.Amount, payment.Method, payment.OperationNumber));
            await paymentRepository.AddAsync(payment);

            if (!result.Approved)
            {
                payment.Fail(result.FailureReason ?? "Rejected by the payment gateway.");
                await unitOfWork.CompleteAsync();
                return;
            }

            payment.Complete(result.TransactionReference, now);
            // Commits the payment and the booking confirmation together (same unit of work and transaction).
            await bookingsContextFacade.ConfirmBookingAsync(booking.BookingId);
            logger.LogInformation("Booking {BookingCode} confirmed by payment {Reference} ({Amount}, {Method}).",
                booking.Code, result.TransactionReference, payment.Amount, payment.Method);
        });
        return payment!;
    }
}
