using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>
///     The user's controller
/// </summary>
/// <remarks>
///     This class is used to handle user requests with full multi-tenancy and scope validation.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Available User endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IUserCommandService userCommandService,
    IMfaCommandService mfaCommandService) : ControllerBase
{
    /// <summary>Resets the two-factor authentication of a user (US-52 scenario 4, e.g. a lost phone).</summary>
    /// <remarks>
    ///     The authenticator and the recovery codes are removed and every session of the user ends. At the next
    ///     sign-in the user must enroll a new authenticator. Audited as <c>MfaReset</c>. Same hierarchy and scope
    ///     rules as the other user management operations (an admin: staff of their hotel).
    /// </remarks>
    [HttpPost("{id:int}/mfa/reset")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Reset the two-factor authentication of a user", OperationId = "ResetUserMfa")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetMfa(int id)
    {
        await mfaCommandService.Handle(new ResetMfaCommand(User.GetUserId(), id));
        return Ok(new MessageResource("Two-factor authentication was reset. The user must set it up again at the next sign-in."));
    }

    /// <summary>
    ///     Creates a new user via management endpoints.
    /// </summary>
    /// <param name="resource">The user creation payload.</param>
    /// <returns>A confirmation message.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Create a user", Description = "US-03 scenario 1: an administrator creates a staff user with a role (admin: reception, housekeeping, maintenance for their own hotel; chain_admin: also admin, any hotel). The user receives a verification e-mail.", OperationId = "CreateUser")]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserResource resource)
    {
        var command = CreateUserCommandFromResourceAssembler.ToCommandFromResource(resource, User.GetUserId());
        var user = await userCommandService.Handle(command);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    /// <summary>Profile of the signed-in user (like OpenID Connect <c>userinfo</c>), read from the account.</summary>
    /// <remarks>
    ///     For profile screens that need fresh account data (names, e-mail, verification, MFA). It is not a way to sync
    ///     permissions: a change of role or hotel ends the user's sessions (401 <c>auth.session_revoked</c>) and the
    ///     new permissions come with the next sign-in. Any role.
    /// </remarks>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get the profile of the signed-in user", Description = "OIDC userinfo-like profile of the account of the access token: id, email, names, role, hotelId, chainId, emailVerified, mfaEnabled.", OperationId = "GetCurrentUser")]
    [ProducesResponseType(typeof(CurrentUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await userQueryService.Handle(new GetCurrentUserQuery(User.GetUserId()))
                   ?? throw new UserNotFoundException(User.GetUserId());
        return Ok(CurrentUserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    /// <summary>
    ///     Get user by id endpoint.
    /// </summary>
    /// <param name="id">The user id to retrieve.</param>
    /// <returns>The enriched user resource.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Get a user by its id", Description = "Retrieves a user only if the actor has scope access to them.", OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found or outside actor's scope")]
    public async Task<IActionResult> GetUserById(int id)
    {

        var getUserByIdQuery = new GetUserByIdQuery(id, User.GetUserId());
        var user = await userQueryService.Handle(getUserByIdQuery);

        // Out-of-scope users are reported as missing so their existence is not disclosed.
        if (user == null) return NotFound();

        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    /// <summary>
    ///     Get all users' endpoint.
    /// </summary>
    /// <returns>The user resources within the actor's scope.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Get all users within scope", Description = "Retrieves all users accessible to the authenticated actor.", OperationId = "GetUsersByScope")]
    [SwaggerResponse(StatusCodes.Status200OK, "The scoped users were found", typeof(IEnumerable<UserResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions")]
    public async Task<IActionResult> GetAllUsers()
    {

        var getUsersByScopeQuery = new GetUsersByScopeQuery(User.GetUserId());
        var users = await userQueryService.Handle(getUsersByScopeQuery);
        
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }

    /// <summary>
    ///     Changes the password for the authenticated user.
    /// </summary>
    /// <param name="resource">The password change resource containing the current and new passwords.</param>
    /// <remarks>UserId is extracted from HttpContext.Items using the established authenticated-user pattern.</remarks>
    /// <returns>An HTTP result indicating whether the password was updated successfully.</returns>
    [HttpPost("change-password")]
    [SwaggerOperation(
        Summary = "Change the authenticated user's password",
        Description = "Requires a valid JWT. UserId is extracted from the token, not from the body.",
        OperationId = "ChangePassword")]
    [SwaggerResponse(StatusCodes.Status200OK, "Password updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data or unexpected error")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated or current password incorrect")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The user was not found")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordResource resource)
    {

        var command = new ChangePasswordCommand(User.GetUserId(), resource.CurrentPassword, resource.NewPassword);
        
        await userCommandService.Handle(command);
        return Ok(new { message = "Password updated successfully" });
    }

    /// <summary>
    ///     Updates an existing user's attributes. All resource fields are optional.
    /// </summary>
    /// <remarks>
    ///     A new hotel or chain ends every session of the user (401 <c>auth.session_revoked</c>,
    ///     <c>reason: "assignment_changed"</c>), sends them the "Tus permisos cambiaron" e-mail and is audited as
    ///     <c>AssignmentChanged</c>.
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Update an existing user", Description = "Updates user attributes if the actor has scope access and hierarchical superiority.", OperationId = "UpdateUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request payload or unexpected error")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions to modify the target")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already registered")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserResource resource)
    {

        var command = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(resource, User.GetUserId(), id);

        await userCommandService.Handle(command);
        return Ok(new { message = "User updated successfully" });
    }

    /// <summary>
    ///     Assigns a new role to an existing user.
    /// </summary>
    /// <remarks>
    ///     US-03 scenario 2: a new role ends every session of the user at once (all access and refresh tokens). Their
    ///     next request gets 401 <c>auth.session_revoked</c> with <c>reason: "role_changed"</c>, they receive the
    ///     e-mail "Tus permisos cambiaron, inicia sesión nuevamente" and sign in again with the new permissions.
    ///     Audited as <c>RoleChanged</c>.
    /// </remarks>
    [HttpPost("{id}/assign-role")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Assign a new role to a user", Description = "Changes a user's role if the actor has scope access and is allowed to assign the target role. The user's sessions end immediately (401 auth.session_revoked, reason role_changed) and they are asked by e-mail to sign in again.", OperationId = "AssignRole")]
    [SwaggerResponse(StatusCodes.Status200OK, "Role assigned successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request payload or unexpected error")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions to assign the role")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleResource resource)
    {

        // Utilizamos el TargetUserId de la ruta como fuente de verdad adaptando el recurso
        // con una expresión 'with' de C#9+ para inyectar la URL como parámetro definitivo.
        var adaptedResource = resource with { TargetUserId = id };
        var command = AssignRoleCommandFromResourceAssembler.ToCommandFromResource(adaptedResource, User.GetUserId());

        await userCommandService.Handle(command);
        return Ok(new { message = "Role assigned successfully" });
    }

    /// <summary>
    ///     Deactivates a user account (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Deactivate a user", Description = "Performs a soft delete on the user if the actor has scope access and hierarchical superiority.", OperationId = "DeactivateUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "User deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Unexpected error")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions to deactivate the target")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> DeactivateUser(int id)
    {

        // DeactivateUserCommand no utiliza recursos provenientes del body,
        // solo el actor ID que viene del contexto de seguridad y el target ID de la ruta.
        var command = new DeactivateUserCommand(User.GetUserId(), id);

        await userCommandService.Handle(command);
        return Ok(new { message = "User deactivated successfully" });
    }

    /// <summary>
    ///     Activates a previously deactivated user account (reverse soft delete).
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Policy = Policies.ManageUsers)]
    [SwaggerOperation(Summary = "Activate a user", Description = "Reactivates a soft-deleted user if the actor has scope access and hierarchical superiority.", OperationId = "ActivateUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "User activated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Unexpected error")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User does not have required permissions to activate the target")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> ActivateUser(int id)
    {

        var command = new ActivateUserCommand(User.GetUserId(), id);

        await userCommandService.Handle(command);
        return Ok(new { message = "User activated successfully" });
    }
}