using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Marketing.Interfaces.REST.Transform;

/// <summary>
///     Codes of the landing contract (US-27) ↔ domain values:
///     accommodationType <c>boutique|alternative|chain</c>, roomsRange <c>1-10|11-30|31-60|60+</c>,
///     referralSource <c>search|social|referral|event|other</c>, profile <c>admin|guest</c>.
/// </summary>
public static class DemoRequestCodes
{
    public static readonly IReadOnlyDictionary<string, AccommodationType> AccommodationTypes = new Dictionary<string, AccommodationType>
    {
        ["boutique"] = AccommodationType.Boutique, ["alternative"] = AccommodationType.Alternative, ["chain"] = AccommodationType.Chain
    };

    public static readonly IReadOnlyDictionary<string, RoomsRange> RoomsRanges = new Dictionary<string, RoomsRange>
    {
        ["1-10"] = RoomsRange.From1To10, ["11-30"] = RoomsRange.From11To30, ["31-60"] = RoomsRange.From31To60, ["60+"] = RoomsRange.MoreThan60
    };

    public static readonly IReadOnlyDictionary<string, ReferralSource> ReferralSources = new Dictionary<string, ReferralSource>
    {
        ["search"] = ReferralSource.Search, ["social"] = ReferralSource.Social, ["referral"] = ReferralSource.Referral,
        ["event"] = ReferralSource.Event, ["other"] = ReferralSource.Other
    };

    public static readonly IReadOnlyDictionary<string, VisitorProfile> Profiles = new Dictionary<string, VisitorProfile>
    {
        ["admin"] = VisitorProfile.Admin, ["guest"] = VisitorProfile.Guest
    };

    public static string Of(AccommodationType value) => AccommodationTypes.Single(pair => pair.Value == value).Key;
    public static string Of(RoomsRange value) => RoomsRanges.Single(pair => pair.Value == value).Key;
    public static string Of(ReferralSource value) => ReferralSources.Single(pair => pair.Value == value).Key;
    public static string Of(VisitorProfile value) => Profiles.Single(pair => pair.Value == value).Key;
}
