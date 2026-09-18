namespace BackendAwSmartstay.API.DemoData.Infrastructure.Configuration;

/// <summary>
///     Demo dataset loaded at startup (section <c>DemoData</c>, env vars <c>DemoData__*</c>). Off by default: a
///     production database starts empty unless the operator opts in.
/// </summary>
public class DemoDataSettings
{
    public const string SectionName = "DemoData";

    /// <summary>Creates the demo dataset after the migrations when it is not there yet (<c>DemoData__Enabled</c>).</summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Password of every seeded account (secret). It must satisfy the password policy of guests (15 characters or
    ///     more), which also covers the staff minimum (8).
    /// </summary>
    public string? DefaultPassword { get; set; }

    /// <summary>
    ///     Mailbox the seeded accounts deliver to, through plus-addressing: <c>local+alias@domain</c> (e.g.
    ///     <c>psulcasanchez+admin1@gmail.com</c>). Without a <c>+</c> of its own.
    /// </summary>
    public string EmailBase { get; set; } = "psulcasanchez@gmail.com";

    /// <summary>
    ///     Payment methods of the first demo hotel (personal data: set by the operator, never invented). Without them
    ///     the hotel has no payment methods, so it does not accept bookings and the demo bookings are skipped.
    /// </summary>
    public DemoHotelPaymentSettings Hotel1Payment { get; set; } = new();
}

/// <summary>Payment methods of a demo hotel (<c>DemoData__Hotel1Payment__*</c>). Validated like the app's form.</summary>
public class DemoHotelPaymentSettings
{
    public string? AccountHolder { get; set; }
    public string? Yape { get; set; }
    public string? Plin { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountCci { get; set; }

    /// <summary>True when the operator set any of the values.</summary>
    public bool IsProvided =>
        new[] { AccountHolder, Yape, Plin, BankName, BankAccountNumber, BankAccountCci }.Any(value => !string.IsNullOrWhiteSpace(value));
}
