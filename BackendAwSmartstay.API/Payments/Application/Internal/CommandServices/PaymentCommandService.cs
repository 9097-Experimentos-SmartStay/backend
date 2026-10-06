using BackendAwSmartstay.API.Payments.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
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
    ICardPaymentGateway cardPaymentGateway,
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
            await EnsurePayableAsync(booking);

            var now = timeProvider.GetUtcNow();
            payment = Payment.Register(booking.BookingId, booking.TotalPrice, command.Method, command.OperationNumber,
                command.Note, command.StaffUserId, now);

            var result = await paymentGateway.ChargeAsync(ChargeFor(booking, payment));
            await SettleAsync(booking, payment, result, now);
        });
        return payment!;
    }

    public async Task<Payment> Handle(PayBookingWithCardCommand command)
    {
        Payment? payment = null;
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Only the guest's own booking: any other is reported as not found.
            var booking = await bookingsContextFacade.FetchBookingAsync(command.BookingId, command.GuestUserId)
                          ?? throw new EntityNotFoundException("Booking", command.BookingId);
            await EnsurePayableAsync(booking);

            var now = timeProvider.GetUtcNow();
            payment = Payment.Register(booking.BookingId, booking.TotalPrice, PaymentMethod.OnlineCard,
                command.Card.MaskedLabel, note: null, command.GuestUserId, now);

            var result = await cardPaymentGateway.ChargeCardAsync(ChargeFor(booking, payment), command.Card);
            await SettleAsync(booking, payment, result, now);
        });

        // The failed attempt is already saved; the guest can retry with another card.
        if (payment!.Status == PaymentStatus.Failed)
            throw new BusinessRuleViolationException(PaymentErrorCodes.CardDeclined, payment.FailureReason!);
        return payment;
    }

    private async Task EnsurePayableAsync(BookingSnapshot booking)
    {
        if (await paymentRepository.ExistsCompletedForBookingAsync(booking.BookingId))
            throw new BusinessRuleViolationException(PaymentErrorCodes.BookingAlreadyPaid, $"Booking {booking.Code} is already paid.");
        if (!booking.CanBePaid)
            throw new BusinessRuleViolationException(PaymentErrorCodes.BookingNotPending,
                $"Booking {booking.Code} is {booking.Status.ToLowerInvariant()}: only a pending booking can be paid.");
    }

    private static PaymentCharge ChargeFor(BookingSnapshot booking, Payment payment) =>
        new(booking.BookingId, booking.Code, payment.Amount, payment.Method, payment.OperationNumber);

    /// <summary>Saves the payment as failed, or completes it and confirms the booking, from the gateway's answer.</summary>
    private async Task SettleAsync(BookingSnapshot booking, Payment payment, PaymentGatewayResult result, DateTimeOffset now)
    {
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
    }
}
