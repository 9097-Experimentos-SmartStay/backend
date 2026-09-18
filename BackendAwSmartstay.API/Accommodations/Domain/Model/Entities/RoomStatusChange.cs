using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

/// <summary>
///     One line of the status history of a room (US-06 scenario 3: date, time and user of every change). Recorded by
///     the <c>Room</c> aggregate in the same transaction as the change; never modified.
/// </summary>
public class RoomStatusChange
{
    public const int MaxEmailLength = 254;

    /// <summary>EF Core constructor.</summary>
    protected RoomStatusChange() { }

    internal RoomStatusChange(int roomId, int hotelId, RoomStatus fromStatus, RoomStatus toStatus,
        RoomStatusChangeOrigin origin, int? changedByUserId, string? changedByEmail, DateTimeOffset changedAt)
    {
        RoomId = roomId;
        HotelId = hotelId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Origin = origin;
        ChangedByUserId = changedByUserId;
        ChangedByEmail = changedByEmail is { Length: > MaxEmailLength } ? changedByEmail[..MaxEmailLength] : changedByEmail;
        ChangedAt = changedAt;
    }

    public long Id { get; private set; }
    public int RoomId { get; private set; }
    public int HotelId { get; private set; }
    public RoomStatus FromStatus { get; private set; }
    public RoomStatus ToStatus { get; private set; }
    public RoomStatusChangeOrigin Origin { get; private set; }

    /// <summary>The staff member (or the guest, for a check-in) who caused the change.</summary>
    public int? ChangedByUserId { get; private set; }

    /// <summary>Their e-mail when the change happened (kept even if the account changes later).</summary>
    public string? ChangedByEmail { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }
}
