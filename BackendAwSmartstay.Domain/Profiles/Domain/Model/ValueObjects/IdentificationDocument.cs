using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public partial record IdentificationDocument
{
    public DocumentType Type { get; }
    public string Number { get; }

    public IdentificationDocument(DocumentType type, string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainValidationException(ProfileErrorCodes.DocumentNumberRequired, "Document number cannot be empty.");

        var trimmed = number.Trim();
        if (type == DocumentType.Dni && !DniRegex().IsMatch(trimmed))
            throw new DomainValidationException(ProfileErrorCodes.DniInvalid, "DNI must contain exactly 8 digits.");

        if (type == DocumentType.Passport && trimmed.Length is < 6 or > 12)
            throw new DomainValidationException(ProfileErrorCodes.PassportInvalid, "Passport must be between 6 and 12 characters.");

        if (type == DocumentType.ForeignerId && trimmed.Length is < 8 or > 15)
            throw new DomainValidationException(ProfileErrorCodes.ForeignerIdInvalid, "Foreigner ID must be between 8 and 15 characters.");

        Type = type;
        Number = trimmed;
    }

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex DniRegex();
}
