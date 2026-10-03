namespace BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

/// <summary>Card entered by the guest. It only lives in memory during the charge: it is never stored nor logged.</summary>
public sealed record CardDetails(string Number, string HolderName, int ExpiryMonth, int ExpiryYear, string Cvv)
{
    public string LastFour => Number[^4..];

    /// <summary>Brand from the card number prefix (Visa, Mastercard, Amex) or "Card".</summary>
    public string Brand => Number switch
    {
        ['4', ..] => "Visa",
        ['3', '4' or '7', ..] => "Amex",
        _ when int.TryParse(Number[..2], out var two) && two is >= 51 and <= 55 => "Mastercard",
        _ when int.TryParse(Number[..4], out var four) && four is >= 2221 and <= 2720 => "Mastercard",
        _ => "Card"
    };

    /// <summary>What a payment keeps of the card, e.g. "Visa ****4242".</summary>
    public string MaskedLabel => $"{Brand} ****{LastFour}";

    /// <summary>Never prints the full number or the CVV.</summary>
    public override string ToString() => MaskedLabel;
}
