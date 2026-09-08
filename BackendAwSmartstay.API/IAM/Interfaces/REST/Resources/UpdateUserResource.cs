using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for updating an existing user. All fields are optional.
/// </summary>
public record UpdateUserResource(
    [MinLength(3)] 
    [MaxLength(100)] 
    [RegularExpression(@"^[a-zA-Z0-9_.@]+$", ErrorMessage = "Username must be alphanumeric and may contain '.', '_', or '@'.")] 
    string? NewUsername,
    
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")] 
    string? NewPassword,
    
    int? NewHotelId,
    
    int? NewChainId
);