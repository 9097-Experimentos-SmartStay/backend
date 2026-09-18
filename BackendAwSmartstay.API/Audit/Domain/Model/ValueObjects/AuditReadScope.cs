namespace BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;

/// <summary>
///     Which entries a reader may see (R4/D2): a chain administrator sees every entry; a hotel administrator only
///     the entries of accounts of their hotel (none when they have no hotel yet).
/// </summary>
public sealed record AuditReadScope
{
    private AuditReadScope(bool allHotels, int? hotelId)
    {
        AllHotels = allHotels;
        HotelId = hotelId;
    }

    public bool AllHotels { get; }
    public int? HotelId { get; }

    public static AuditReadScope Everything() => new(true, null);

    public static AuditReadScope OfHotel(int? hotelId) => new(false, hotelId);

    /// <summary>True when an entry about an account of <paramref name="entryHotelId"/> is visible.</summary>
    public bool Includes(int? entryHotelId) => AllHotels || (HotelId is not null && entryHotelId == HotelId);
}
