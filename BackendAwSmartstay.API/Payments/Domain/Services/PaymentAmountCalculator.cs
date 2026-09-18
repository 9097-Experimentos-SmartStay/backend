namespace BackendAwSmartstay.API.Payments.Domain.Services;

/// <summary>
///     Domain service that computes how much a booking costs. The client never decides the amount.
/// </summary>
public static class PaymentAmountCalculator
{
    /// <summary>
    ///     Number of nights between check-in and check-out (calendar days, time of day ignored).
    /// </summary>
    public static int CalculateNights(DateTime checkInDate, DateTime checkOutDate) =>
        (checkOutDate.Date - checkInDate.Date).Days;

    /// <summary>
    ///     Total amount = price per night × nights, rounded to 2 decimals.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Negative price or less than one night.</exception>
    public static decimal Calculate(decimal pricePerNight, DateTime checkInDate, DateTime checkOutDate)
    {
        if (pricePerNight < 0)
            throw new ArgumentOutOfRangeException(nameof(pricePerNight), "The nightly price cannot be negative.");

        var nights = CalculateNights(checkInDate, checkOutDate);
        if (nights < 1)
            throw new ArgumentOutOfRangeException(nameof(checkOutDate), "A booking must cover at least one night.");

        return decimal.Round(pricePerNight * nights, 2, MidpointRounding.AwayFromZero);
    }
}
