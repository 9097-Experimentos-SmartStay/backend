using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for creating a new user via management endpoints.
/// </summary>
public record CreateUserResource(
    [Required] 
    [MinLength(3)] 
    [MaxLength(100)] 
    [RegularExpression(@"^[a-zA-Z0-9_.@]+$", ErrorMessage = "Username must be alphanumeric and may contain '.', '_', or '@'.")] 
    string Username,
    
    [Required] 
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")] 
    string Password,
    
    [Required] 
    string Role,
    
    int? HotelId,
    
    int? ChainId
);