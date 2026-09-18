using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public record PersonName
{
    public string FirstName { get; }
    public string LastName { get; }

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainValidationException(ProfileErrorCodes.FirstNameRequired, "First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainValidationException(ProfileErrorCodes.LastNameRequired, "Last name cannot be empty.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";
}