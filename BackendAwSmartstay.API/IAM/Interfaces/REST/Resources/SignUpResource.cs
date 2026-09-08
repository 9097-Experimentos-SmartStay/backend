using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource definition for user registration data.
/// </summary>
/// <param name="Username">The desired username (email).</param>
/// <param name="Password">The desired password.</param>
/// <param name="Role">The optional role to assign.</param>
public record SignUpResource(
    [Required] 
    [MinLength(3)] 
    [MaxLength(100)] 
    [RegularExpression(@"^[a-zA-Z0-9_.@]+$", ErrorMessage = "Username must be alphanumeric and may contain '.', '_', or '@'.")] 
    string Username, 
    
    [Required] 
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")] 
    string Password,
    
    string? Role
);