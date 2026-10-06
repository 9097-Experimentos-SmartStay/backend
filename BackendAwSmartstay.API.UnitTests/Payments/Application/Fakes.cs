using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.UnitTests.Payments.Application;

/// <summary>Hand-written test doubles for the Payments application layer (the project has no mocking library).</summary>
internal sealed class FakePaymentRepository : IPaymentRepository
{
    public List<Payment> Payments { get; } = [];

    public Task AddAsync(Payment entity)
    {
        Payments.Add(entity);
        return Task.CompletedTask;
    }

    public Task<Payment?> FindByIdAsync(int id) => Task.FromResult(Payments.FirstOrDefault(p => p.Id == id));

    public void Update(Payment entity) { }

    public void Remove(Payment entity) => Payments.Remove(entity);

    public Task<IEnumerable<Payment>> ListAsync() => Task.FromResult<IEnumerable<Payment>>(Payments);

    public Task<Payment?> FindByBookingIdAsync(int bookingId) =>
        Task.FromResult(Payments.Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.Status == PaymentStatus.Completed).ThenByDescending(p => p.PaymentDate)
            .FirstOrDefault());

    public Task<bool> ExistsCompletedForBookingAsync(int bookingId) =>
        Task.FromResult(Payments.Any(p => p.BookingId == bookingId && p.Status == PaymentStatus.Completed));

    public Task<Payment?> FindCompletedByBookingIdAsync(int bookingId) =>
        Task.FromResult(Payments.FirstOrDefault(p => p.BookingId == bookingId && p.Status == PaymentStatus.Completed));
}

internal sealed class FakeBookingsFacade : IBookingsContextFacade
{
    public BookingSnapshot? Booking { get; set; }
    public int? LastGuestUserIdRequested { get; private set; }
    public List<int> ConfirmedBookingIds { get; } = [];

    public Task<BookingSnapshot?> FetchBookingAsync(int bookingId, int? guestUserId = null)
    {
        LastGuestUserIdRequested = guestUserId;
        // Mirrors the real facade: with a guest id only that guest's booking is visible.
        var visible = Booking is not null && Booking.BookingId == bookingId
                      && (guestUserId is null || guestUserId == OwnerGuestId);
        return Task.FromResult(visible ? Booking : null);
    }

    public int OwnerGuestId { get; set; } = 7;

    public Task<bool> ConfirmBookingAsync(int bookingId)
    {
        ConfirmedBookingIds.Add(bookingId);
        return Task.FromResult(true);
    }

    public Task<bool> HasCurrentConfirmedStayAsync(int guestUserId, int roomId, DateTime day) =>
        Task.FromResult(false);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CompleteCalls { get; private set; }
    public int TransactionCalls { get; private set; }

    public Task CompleteAsync()
    {
        CompleteCalls++;
        return Task.CompletedTask;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> work)
    {
        TransactionCalls++;
        await work();
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class StubPaymentGateway(PaymentGatewayResult result) : IPaymentGateway
{
    public PaymentCharge? LastCharge { get; private set; }

    public Task<PaymentGatewayResult> ChargeAsync(PaymentCharge charge, CancellationToken cancellationToken = default)
    {
        LastCharge = charge;
        return Task.FromResult(result);
    }
}

internal sealed class StubCardPaymentGateway(PaymentGatewayResult result) : ICardPaymentGateway
{
    public PaymentCharge? LastCharge { get; private set; }
    public CardDetails? LastCard { get; private set; }

    public Task<PaymentGatewayResult> ChargeCardAsync(PaymentCharge charge, CardDetails card,
        CancellationToken cancellationToken = default)
    {
        LastCharge = charge;
        LastCard = card;
        return Task.FromResult(result);
    }
}
