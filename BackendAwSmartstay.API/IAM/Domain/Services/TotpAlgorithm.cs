using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>
///     Time-based one-time passwords, RFC 6238 with the parameters every authenticator app supports: 30-second
///     steps, 6 digits, HMAC-SHA1. A code is accepted in the current step and one step before or after (clock drift).
/// </summary>
public static class TotpAlgorithm
{
    public const int PeriodSeconds = 30;
    public const int Digits = 6;
    public const string Algorithm = "SHA1";

    /// <summary>Steps accepted before and after the current one.</summary>
    public const int AllowedDriftSteps = 1;

    /// <summary>Number of the 30-second step that contains <paramref name="at"/> (T in RFC 6238).</summary>
    public static long TimeStepAt(DateTimeOffset at) => at.ToUnixTimeSeconds() / PeriodSeconds;

    /// <summary>The code of <paramref name="secret"/> for <paramref name="timeStep"/> (RFC 4226 HOTP).</summary>
    public static string CodeAt(TotpSecret secret, long timeStep)
    {
        Span<byte> counter = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);
        var hash = secret.Sign(counter);
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6");
    }

    /// <summary>
    ///     The time step whose code is <paramref name="code"/> within the allowed drift around <paramref name="now"/>,
    ///     or null when it matches none. Comparisons are constant-time.
    /// </summary>
    public static long? MatchingTimeStep(TotpSecret secret, string code, DateTimeOffset now)
    {
        var candidate = Encoding.ASCII.GetBytes(code);
        var current = TimeStepAt(now);
        long? match = null;
        for (var step = current - AllowedDriftSteps; step <= current + AllowedDriftSteps; step++)
        {
            // Every step is evaluated so the time taken does not reveal which one matched.
            if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(CodeAt(secret, step)), candidate))
                match ??= step;
        }
        return match;
    }
}
