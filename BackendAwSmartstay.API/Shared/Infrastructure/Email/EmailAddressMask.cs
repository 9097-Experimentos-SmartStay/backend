namespace BackendAwSmartstay.API.Shared.Infrastructure.Email;

/// <summary>Masks an e-mail address for the logs: <c>jane.doe@example.com</c> → <c>j***@example.com</c>.</summary>
public static class EmailAddressMask
{
    public static string Mask(string address)
    {
        var at = address.LastIndexOf('@');
        if (at <= 0) return "***";
        return $"{address[0]}***{address[at..]}";
    }
}
