using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     Reference to the guest account (IAM user) a booking belongs to (<c>guestId</c> in the report's Booking
///     component). Exposed in the API as <c>userId</c>.
/// </summary>
public sealed record GuestId
{
    public GuestId(int value)
    {
        if (value <= 0)
            throw new DomainValidationException("A guest reference must be a positive user id.");
        Value = value;
    }

    public int Value { get; }

    public override string ToString() => Value.ToString();
}
