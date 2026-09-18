namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>Why a booking was cancelled.</summary>
public enum CancellationReason
{
    /// <summary>The guest cancelled it.</summary>
    GuestRequest,
    /// <summary>Hotel staff cancelled it (US-07 scenario 4).</summary>
    HotelRequest,
    /// <summary>No payment was registered before the payment deadline (payment hold expired).</summary>
    PaymentNotReceived
}
