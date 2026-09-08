using System.Net.Mime;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
{
    /// <summary>
    ///     Sign in endpoint.
    /// </summary>
    /// <param name="signInResource">The sign-in resource containing username and password.</param>
    /// <returns>The authenticated user resource, including a JWT token</returns>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Sign in", Description = "Sign in a user", OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was authenticated", typeof(AuthenticatedUserResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);

        try
        {
            var authenticatedUser = await userCommandService.Handle(signInCommand);
            var resource = AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(
                authenticatedUser.user, authenticatedUser.token);
            return Ok(resource);
        }
        catch (InvalidCredentialsException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    ///     Sign up endpoint.
    /// </summary>
    /// <param name="signUpResource">The sign-up resource containing username and password.</param>
    /// <returns>A confirmation message on successful creation.</returns>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Sign-up", Description = "Sign up a new user. If a Role is provided, valid JWT authentication is required.", OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was created successfully")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Authentication required or insufficient permissions to assign the requested role")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Username already exists")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        var actor = (User?)HttpContext.Items["User"];
        var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource, actor?.Id);

        try
        {
            await userCommandService.Handle(signUpCommand);
            return Ok(new { message = "User created successfully" });
        }
        catch (UsernameAlreadyExistsException e)
        {
            return Conflict(new { message = e.Message });
        }
        catch (UnauthorizedOperationException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = e.Message });
        }
    }
}