using BackendAwSmartstay.API.Accommodations.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;

/// <summary>
/// Represents a room aggregate in the accommodations domain: its price and its operational status (US-06, US-29).
/// </summary>
public partial class Room : IHasDomainEvents
{
    private readonly List<IEvent> _domainEvents = [];

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Initializes a new instance of the <see cref="Room"/> class with default values.
    /// </summary>
    public Room()
    {
        Description = string.Empty;
        Amenities = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Room"/> class from a create command.
    /// </summary>
    /// <param name="command">The command containing room creation data.</param>
    public Room(CreateRoomCommand command) : this()
    {
        Number = RoomNumber.Normalize(command.Number);
        EnsureValidPrice(command.Price);

        RoomTypeId = command.RoomTypeId;
        // NUEVOS CAMPOS
        HotelId = command.HotelId;
        Price = command.Price;
        // -------------
        Description = command.Description;
        Amenities = command.Amenities;
        Status = RoomStatus.Available;
        StatusChangedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     The number staff and guests use for the room (e.g. "101", "2B"), unique within its hotel (US-53). It is what
    ///     the room map shows (US-06).
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>Renumbers the room (uniqueness in the hotel is checked by the application service).</summary>
    public void Renumber(string number) => Number = RoomNumber.Normalize(number);

    /// <summary>US-53: a room is sold for a positive price per night.</summary>
    private static void EnsureValidPrice(decimal price)
    {
        if (price <= 0)
            throw new InvalidFieldException("price", AccommodationErrorCodes.RoomPriceOutOfRange, "The price per night must be greater than 0.");
        if (price > RoomNumber.MaxPrice)
            throw new InvalidFieldException("price", AccommodationErrorCodes.RoomPriceOutOfRange, $"The price per night cannot exceed {RoomNumber.MaxPrice}.");
    }

    /// <summary>Operational status (US-29). New rooms are Available.</summary>
    public RoomStatus Status { get; private set; } = RoomStatus.Available;

    /// <summary>Since when the room has its current status (US-06 map and maintenance alert).</summary>
    public DateTimeOffset StatusChangedAt { get; private set; }

    /// <summary>When the overdue-maintenance alert of the current maintenance period was sent.</summary>
    public DateTimeOffset? MaintenanceAlertSentAt { get; private set; }

    /// <summary>A room under maintenance is never offered for booking.</summary>
    public bool IsOfferedForBooking => Status != RoomStatus.Maintenance;

    /// <summary>
    ///     Moves the room to <paramref name="newStatus"/> if the transition is valid and returns the history line to
    ///     store with it (US-06 scenario 3), or null when the status does not change.
    /// </summary>
    /// <exception cref="InvalidRoomStatusTransitionException">The transition is not allowed.</exception>
    public RoomStatusChange? ChangeStatus(RoomStatus newStatus, RoomStatusChangeOrigin origin, int? changedByUserId,
        string? changedByEmail, DateTimeOffset now)
    {
        if (!RoomStatusTransitions.IsAllowed(Status, newStatus))
            throw new InvalidRoomStatusTransitionException(Id, Status, newStatus);
        if (newStatus == Status) return null;

        var change = new RoomStatusChange(Id, HotelId, Status, newStatus, origin, changedByUserId, changedByEmail, now);
        _domainEvents.Add(new RoomStatusChangedEvent(Id, HotelId, Status, newStatus, origin, changedByUserId, now));
        Status = newStatus;
        StatusChangedAt = now;
        MaintenanceAlertSentAt = null;
        return change;
    }

    /// <summary>US-08: the guest of a completed check-in moves in. Only an Available room can be occupied.</summary>
    public RoomStatusChange OccupyForCheckIn(int? guestUserId, string? guestEmail, DateTimeOffset now)
    {
        if (Status != RoomStatus.Available)
            throw new RoomNotReadyForCheckInException(Id, Status);
        return ChangeStatus(RoomStatus.Occupied, RoomStatusChangeOrigin.CheckIn, guestUserId, guestEmail, now)!;
    }

    /// <summary>
    ///     US-06 scenario 4: the room has been under maintenance for at least <paramref name="threshold"/> and no alert
    ///     was sent for this maintenance period yet.
    /// </summary>
    public bool IsMaintenanceAlertDue(DateTimeOffset now, TimeSpan threshold) =>
        Status == RoomStatus.Maintenance && now - StatusChangedAt >= threshold && MaintenanceAlertSentAt is null;

    /// <summary>Records the overdue-maintenance alert (once per maintenance period).</summary>
    public bool RaiseMaintenanceAlert(DateTimeOffset now, TimeSpan threshold)
    {
        if (!IsMaintenanceAlertDue(now, threshold)) return false;
        MaintenanceAlertSentAt = now;
        _domainEvents.Add(new RoomMaintenanceOverdueEvent(Id, HotelId, StatusChangedAt, now));
        return true;
    }
    
    /// <summary>
    /// Updates the mutable information of the room aggregate.
    /// This method enforces business invariants during updates.
    /// </summary>
    /// <param name="roomTypeId">The new room type identifier.</param>
    /// <param name="price">The new price per night.</param>
    /// <param name="description">The new description.</param>
    /// <param name="amenities">The new list of amenities.</param>
    public void UpdateInformation(int roomTypeId, decimal price, string description, List<string> amenities)
    {
        EnsureValidPrice(price);

        RoomTypeId = roomTypeId;
        Price = price;
        Description = description;
        Amenities = amenities;
    }

    /// <summary>
    /// The unique identifier of the room.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// The identifier of the room type.
    /// </summary>
    public int RoomTypeId { get; private set; }
    
    // NUEVOS CAMPOS
    /// <summary>
    /// The identifier of the hotel this room belongs to.
    /// </summary>
    public int HotelId { get; private set; } // FK al Hotel
    /// <summary>
    /// The price per night for the room.
    /// </summary>
    public decimal Price { get; private set; } // Precio por noche
    // -------------
    
    /// <summary>
    /// A description of the room.
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// The list of amenities provided by the room.
    /// </summary>
    public List<string> Amenities { get; private set; }

    // Navigation Properties
    /// <summary>
    /// The room type associated with this room.
    /// </summary>
    public virtual RoomType RoomType { get; private set; } = null!;
    /// <summary>
    /// The hotel this room belongs to.
    /// </summary>
    public virtual Hotel Hotel { get; private set; } = null!;
}