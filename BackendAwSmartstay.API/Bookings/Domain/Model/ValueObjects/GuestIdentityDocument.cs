using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>Identity documents accepted at the digital check-in (US-08).</summary>
public enum IdentityDocumentType
{
    /// <summary>Peruvian national identity document: exactly 8 digits; only for Peruvian nationals.</summary>
    Dni,
    /// <summary>Passport: 6 to 12 letters and digits.</summary>
    Passport,
    /// <summary>Carné de extranjería (Peruvian foreigner card): 8 to 12 letters and digits; only for foreigners.</summary>
    Ce
}

/// <summary>
///     The identity the guest declares at the digital check-in (US-08 scenario 2). The API validates the FORMAT of
///     the document (no OCR, no registry lookup): type, number and nationality must be consistent.
/// </summary>
public sealed partial record GuestIdentityDocument
{
    public const string Peru = "PE";

    public GuestIdentityDocument(IdentityDocumentType type, string? number, string? nationality)
    {
        var normalizedNationality = nationality?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!IsoCountryCodes.IsValid(normalizedNationality))
            throw new InvalidFieldException("nationality", BookingErrorCodes.CheckInNationalityInvalid, "Nationality must be an ISO 3166-1 alpha-2 country code, e.g. PE, AR, US.");

        var normalizedNumber = Separators().Replace(number?.Trim() ?? string.Empty, string.Empty).ToUpperInvariant();
        switch (type)
        {
            case IdentityDocumentType.Dni:
                if (!Dni().IsMatch(normalizedNumber))
                    throw new InvalidFieldException("documentNumber", BookingErrorCodes.CheckInDniInvalid, "A DNI has exactly 8 digits.");
                if (normalizedNationality != Peru)
                    throw new InvalidFieldException("documentType", BookingErrorCodes.CheckInDniOnlyForNationals, "The DNI is only for Peruvian nationals (nationality PE). Use your passport or carné de extranjería.");
                break;
            case IdentityDocumentType.Passport:
                if (!Passport().IsMatch(normalizedNumber))
                    throw new InvalidFieldException("documentNumber", BookingErrorCodes.CheckInPassportInvalid, "A passport number has 6 to 12 letters and digits.");
                break;
            case IdentityDocumentType.Ce:
                if (!ForeignerCard().IsMatch(normalizedNumber))
                    throw new InvalidFieldException("documentNumber", BookingErrorCodes.CheckInForeignerCardInvalid, "A carné de extranjería number has 8 to 12 letters and digits.");
                if (normalizedNationality == Peru)
                    throw new InvalidFieldException("documentType", BookingErrorCodes.CheckInForeignerCardOnlyForForeigners, "The carné de extranjería is for foreign nationals. Peruvian nationals use their DNI.");
                break;
            default:
                throw new InvalidFieldException("documentType", BookingErrorCodes.CheckInDocumentTypeUnknown, "Document type must be DNI, PASSPORT or CE.");
        }

        Type = type;
        Number = normalizedNumber;
        Nationality = normalizedNationality;
    }

    public IdentityDocumentType Type { get; }
    public string Number { get; }
    public string Nationality { get; }

    /// <summary>The number with all but the last 3 characters hidden, for staff views.</summary>
    public string MaskedNumber => new string('•', Math.Max(0, Number.Length - 3)) + Number[^Math.Min(3, Number.Length)..];

    [GeneratedRegex(@"[\s.-]")]
    private static partial Regex Separators();

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex Dni();

    [GeneratedRegex(@"^[A-Z0-9]{6,12}$")]
    private static partial Regex Passport();

    [GeneratedRegex(@"^[A-Z0-9]{8,12}$")]
    private static partial Regex ForeignerCard();
}

/// <summary>ISO 3166-1 alpha-2 country codes.</summary>
public static class IsoCountryCodes
{
    private static readonly HashSet<string> Codes = new(("AD AE AF AG AI AL AM AO AQ AR AS AT AU AW AX AZ BA BB BD BE BF BG BH BI BJ BL BM BN BO BQ " +
        "BR BS BT BV BW BY BZ CA CC CD CF CG CH CI CK CL CM CN CO CR CU CV CW CX CY CZ DE DJ DK DM DO DZ EC EE EG EH ER ES " +
        "ET FI FJ FK FM FO FR GA GB GD GE GF GG GH GI GL GM GN GP GQ GR GS GT GU GW GY HK HM HN HR HT HU ID IE IL IM IN IO " +
        "IQ IR IS IT JE JM JO JP KE KG KH KI KM KN KP KR KW KY KZ LA LB LC LI LK LR LS LT LU LV LY MA MC MD ME MF MG MH MK " +
        "ML MM MN MO MP MQ MR MS MT MU MV MW MX MY MZ NA NC NE NF NG NI NL NO NP NR NU NZ OM PA PE PF PG PH PK PL PM PN PR " +
        "PS PT PW PY QA RE RO RS RU RW SA SB SC SD SE SG SH SI SJ SK SL SM SN SO SR SS ST SV SX SY SZ TC TD TF TG TH TJ TK " +
        "TL TM TN TO TR TT TV TW TZ UA UG UM US UY UZ VA VC VE VG VI VN VU WF WS YE YT ZA ZM ZW").Split(' '), StringComparer.Ordinal);

    public static bool IsValid(string code) => Codes.Contains(code);
}
