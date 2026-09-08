namespace BackendAwSmartstay.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Email address value object
/// </summary>
/// <param name="Address">
///     The email address
/// </param>
public record EmailAddress(string Address)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailAddress" /> record with an empty address.
    /// </summary>
    public EmailAddress() : this(string.Empty)
    {
    }
}