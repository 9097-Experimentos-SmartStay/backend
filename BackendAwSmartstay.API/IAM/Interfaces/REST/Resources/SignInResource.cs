using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource for signing in a user.
/// </summary>
/// <param name="Username">The username of the user.</param>
/// <param name="Password">The password of the user.</param>
public record SignInResource(
    [Required] 
    [MinLength(3)] 
    [MaxLength(100)] 
    [RegularExpression(@"^[a-zA-Z0-9_.@]+$", ErrorMessage = "Username must be alphanumeric and may contain '.', '_', or '@'.")] 
    string Username, 
    
    [Required] 
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")] 
    string Password
);