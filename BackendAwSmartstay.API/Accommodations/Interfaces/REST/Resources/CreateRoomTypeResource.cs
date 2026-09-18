using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>A new room type used to classify rooms (US-53 scenario 2).</summary>
public record CreateRoomTypeResource
{
    /// <summary>Name, 2 to 50 characters.</summary>
    /// <example>Suite Jr</example>
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>Description, up to 500 characters.</summary>
    /// <example>Suite junior con vista al mar</example>
    [Required, MaxLength(500)]
    public string Description { get; init; } = string.Empty;
}
