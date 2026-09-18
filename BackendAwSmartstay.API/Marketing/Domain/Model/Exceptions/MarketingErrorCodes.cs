namespace BackendAwSmartstay.API.Marketing.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Marketing context (demo requests, US-27).</summary>
public static class MarketingErrorCodes
{
    public const string FieldRequired = "demo_request.field_required";
    public const string FieldLength = "demo_request.field_length";
    public const string NameInvalid = "demo_request.name_invalid";
    public const string EmailInvalid = "demo_request.email_invalid";
    public const string PhoneInvalid = "demo_request.phone_invalid";
    public const string MessageTooLong = "demo_request.message_too_long";
}
