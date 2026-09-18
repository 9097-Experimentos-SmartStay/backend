using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for updating an existing user. All fields are optional.
/// </summary>
public record UpdateUserResource
{
    [EmailAddress]
    [MaxLength(254)]
    public string? NewEmail { get; init; }

    /// <summary>Deprecated alias of <see cref="NewEmail"/>, accepted for backward compatibility.</summary>
    [EmailAddress]
    [MaxLength(254)]
    public string? NewUsername { get; init; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? NewPassword { get; init; }

    public int? NewHotelId { get; init; }

    public int? NewChainId { get; init; }

    [JsonIgnore]
    public string? LoginEmail => string.IsNullOrWhiteSpace(NewEmail) ? NewUsername : NewEmail;
}
