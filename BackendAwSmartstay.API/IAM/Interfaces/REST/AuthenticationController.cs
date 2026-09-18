using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>
/// Controller for authentication operations. Both endpoints are anonymous.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
{
    /// <summary>
    ///     Sign in endpoint.
    /// </summary>
    /// <param name="signInResource">The sign-in resource containing e-mail and password.</param>
    /// <returns>The authenticated user resource, including a JWT token</returns>
    [HttpPost("sign-in")]
    [SwaggerOperation(Summary = "Sign in", Description = "Sign in a user", OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was authenticated", typeof(AuthenticatedUserResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The account is deactivated")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
        var (user, token) = await userCommandService.Handle(signInCommand);
        return Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(user, token));
    }

    /// <summary>
    ///     Sign up endpoint. Anonymous; a bearer token is optional and only needed to assign a non-guest role.
    /// </summary>
    /// <param name="signUpResource">The sign-up resource containing e-mail and password.</param>
    /// <returns>A confirmation message on successful creation.</returns>
    [HttpPost("sign-up")]
    [SwaggerOperation(Summary = "Sign-up", Description = "Sign up a new user. If a non-guest Role is provided, a valid JWT of a user allowed to assign it is required.", OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was created successfully")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Authentication required or insufficient permissions to assign the requested role")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already registered")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        // The authentication handler still runs on anonymous endpoints: a valid token identifies the actor.
        var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource, User.FindUserId());
        await userCommandService.Handle(signUpCommand);
        return Ok(new { message = "User created successfully" });
    }
}
