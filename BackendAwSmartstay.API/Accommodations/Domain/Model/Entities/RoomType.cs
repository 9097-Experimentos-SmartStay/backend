using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

/// <summary>
/// Represents a room type entity in the accommodations domain.
/// </summary>
public class RoomType
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomType"/> class with default values.
    /// </summary>
    public RoomType()
    {
        Name = string.Empty;
        Description = string.Empty;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomType"/> class with specified name and description.
    /// </summary>
    /// <param name="name">The name of the room type.</param>
    /// <param name="description">The description of the room type.</param>
    public RoomType(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomType"/> class from a create command.
    /// </summary>
    /// <param name="command">The command containing room type creation data.</param>
    public RoomType(CreateRoomTypeCommand command)
    {
        Name = command.Name;
        Description = command.Description;
    }

    /// <summary>
    /// The unique identifier of the room type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the room type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the room type.
    /// </summary>
    public string Description { get; set; }
}
