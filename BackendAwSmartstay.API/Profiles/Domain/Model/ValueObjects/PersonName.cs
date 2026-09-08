namespace BackendAwSmartstay.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value object for a person's name
/// </summary>
/// <param name="FirstName">
///     The first name of the person
/// </param>
/// <param name="LastName">
///     The last name of the person
/// </param>
public record PersonName(string FirstName, string LastName)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PersonName" /> record with empty names.
    /// </summary>
    public PersonName() : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersonName" /> record with the specified first name and empty last
    ///     name.
    /// </summary>
    /// <param name="firstName">The first name of the person.</param>
    public PersonName(string firstName) : this(firstName, string.Empty)
    {
    }

    /// <summary>
    ///     Gets the full name of the person.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}