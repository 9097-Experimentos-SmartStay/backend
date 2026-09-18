using System.Security.Cryptography;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     Digital access to the room given at the check-in (US-08 scenario 1): a random 6-digit code for the door keypad,
///     valid until the check-out.
/// </summary>
public sealed record RoomAccessCode(string Value, DateTimeOffset ValidUntil)
{
    public const int Digits = 6;

    public static RoomAccessCode Generate(DateTimeOffset validUntil) =>
        new(RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6"), validUntil);
}
