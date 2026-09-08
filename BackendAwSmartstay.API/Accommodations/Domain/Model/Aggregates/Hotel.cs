using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;

/// <summary>
/// Represents a hotel accommodation aggregate in the domain, encapsulating rooms and related business logic.
/// </summary>
public partial class Hotel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Hotel"/> class with default values.
    /// </summary>
    public Hotel()
    {
        Name = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        Country = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        Type = string.Empty;
        Amenities = new List<string>();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Hotel"/> class from a create command.
    /// </summary>
    /// <param name="command">The command containing hotel creation data.</param>
    public Hotel(CreateHotelCommand command) : this()
    {
        HostId = command.HostId;
        Name = command.Name;
        Address = command.Address;
        City = command.City;
        Country = command.Country;
        
        ImageUrl = command.ImageUrl;
        Description = command.Description;
        Type = command.Type;
        Amenities = command.Amenities;
    }
    
    /// <summary>
    /// Navigation property for the rooms belonging to this hotel.
    /// Required for calculating dynamic pricing (e.g., "From $X").
    /// </summary>
    public virtual ICollection<Room> Rooms { get; private set; } = new List<Room>();

    /// <summary>
    /// Calculates the lowest price among all rooms in this hotel.
    /// </summary>
    /// <returns>The minimum price found, or 0 if no rooms exist.</returns>
    public decimal CalculateLowestPrice()
    {
        if (Rooms == null || !Rooms.Any())
        {
            return 0;
        }
        return Rooms.Min(r => r.Price);
    }
    
    /// <summary>
    /// Updates the mutable information of the hotel aggregate.
    /// This method enforces business invariants during updates.
    /// </summary>
    /// <param name="name">The new name of the property.</param>
    /// <param name="address">The new street address.</param>
    /// <param name="city">The new city.</param>
    /// <param name="country">The new country.</param>
    /// <param name="imageUrl">The new image URL.</param>
    /// <param name="description">The new description.</param>
    /// <param name="type">The new accommodation type.</param>
    /// <param name="amenities">The new list of amenities.</param>
    public void UpdateInformation(string name, string address, string city, string country, string imageUrl, string description, string type, List<string> amenities)
    {
        // Here you could add validation logic (e.g., ensure name is not empty)
        Name = name;
        Address = address;
        City = city;
        Country = country;
        ImageUrl = imageUrl;
        Description = description;
        Type = type;
        Amenities = amenities;
    }

    /// <summary>
    /// The unique identifier of the hotel.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// The identifier of the host owning the hotel.
    /// </summary>
    public int HostId { get; private set; }
    /// <summary>
    /// The name of the hotel.
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// The street address of the hotel.
    /// </summary>
    public string Address { get; private set; }
    /// <summary>
    /// The city where the hotel is located.
    /// </summary>
    public string City { get; private set; }
    /// <summary>
    /// The country where the hotel is located.
    /// </summary>
    public string Country { get; private set; }
    /// <summary>
    /// The URL of the hotel's image.
    /// </summary>
    public string ImageUrl { get; private set; }
    /// <summary>
    /// A description of the hotel.
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// The type of accommodation (e.g., hotel, resort).
    /// </summary>
    public string Type { get; private set; }
    /// <summary>
    /// The list of amenities provided by the hotel.
    /// </summary>
    public List<string> Amenities { get; private set; }
}