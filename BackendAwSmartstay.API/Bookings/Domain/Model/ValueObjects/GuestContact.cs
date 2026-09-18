using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     Who is staying and how to reach them (US-07 scenario 2: a reservation taken by phone). A booking keeps this
///     snapshot even when there is no guest account: a walk-in or phone guest must not be forced to register, and the
///     e-mail is what the booking notifications go to. When the guest has an account the booking also references it.
/// </summary>
public sealed partial record GuestContact
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 200;

    public GuestContact(string? name, string? email, string? phone)
    {
        var trimmedName = Collapse(name);
        if (trimmedName.Length is < 2 or > MaxNameLength)
            throw new InvalidFieldException("guestName", BookingErrorCodes.GuestNameInvalid, $"Enter the guest's name (2 to {MaxNameLength} characters).");

        var trimmedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (trimmedEmail.Length > MaxEmailLength || !EmailFormat().IsMatch(trimmedEmail))
            throw new InvalidFieldException("guestEmail", BookingErrorCodes.GuestEmailInvalid, "Enter a valid guest e-mail (for example name@domain.com): the booking e-mails go there.");

        string? normalizedPhone = null;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            normalizedPhone = PhoneSeparators().Replace(phone.Trim(), string.Empty);
            if (!PhoneFormat().IsMatch(normalizedPhone))
                throw new InvalidFieldException("guestPhone", BookingErrorCodes.GuestPhoneInvalid, "The phone must have 7 to 15 digits and may start with +.");
        }

        Name = trimmedName;
        Email = trimmedEmail;
        Phone = normalizedPhone;
    }

    public string Name { get; }
    public string Email { get; }
    public string? Phone { get; }

    private static string Collapse(string? value) => WhiteSpace().Replace(value?.Trim() ?? string.Empty, " ");

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailFormat();

    [GeneratedRegex(@"^\+?[0-9]{7,15}$")]
    private static partial Regex PhoneFormat();

    [GeneratedRegex(@"[\s().-]")]
    private static partial Regex PhoneSeparators();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpace();
}
