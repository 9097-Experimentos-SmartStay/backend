using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>New status of a room (US-29 scenario 2).</summary>
public record ChangeRoomStatusResource : IValidatableObject
{
    /// <summary>Available, Occupied, Cleaning or Maintenance (case-insensitive).</summary>
    /// <example>Cleaning</example>
    [Required]
    public string? Status { get; init; }

    /// <summary>The validated status as a domain value (a method, so model validation never evaluates it).</summary>
    public RoomStatus ToRoomStatus() => Enum.Parse<RoomStatus>(Status!, ignoreCase: true);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Status)
            && (!Enum.TryParse<RoomStatus>(Status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed) || int.TryParse(Status, out _)))
            yield return new CodedValidationResult(ErrorCodes.FieldNotAllowed,
                $"Status must be one of: {string.Join(", ", Enum.GetNames<RoomStatus>())}.", ["status"],
                new Dictionary<string, object?> { ["allowed"] = Enum.GetNames<RoomStatus>() });
    }
}
