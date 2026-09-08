using System.Net.Mime;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using BackendAwSmartstay.API.Profiles.Domain.Model.Queries;
using BackendAwSmartstay.API.Profiles.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling operational, guest, and administrative
///     requests related to user personal identity profile aggregates within the profiles bounded context.
/// </summary>
/// <param name="profileCommandService">The domain command service used to handle profile creation and mutations.</param>
/// <param name="profileQueryService">The domain query service used to handle profile state extraction and tracking queries.</param>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Profile Endpoints.")]
public class ProfilesController(
    IProfileCommandService profileCommandService,
    IProfileQueryService profileQueryService)
    : ControllerBase
{
    /// <summary>
    ///     Retrieves a single personal identity profile resource partition by its technical identity marker.
    /// </summary>
    /// <param name="profileId">The unique structural domain identifier value representing the targeted profile aggregate root.</param>
    /// <returns>An asynchronous action result containing the matching profile resource state representation, or NotFound.</returns>
    [HttpGet("{profileId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get profile by its unique identifier", 
        Description = "Retrieves structural identity fields and personal metrics for a single profile aggregate entry. Restricted to administrative operators.", 
        OperationId = "GetProfileById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The profile aggregate partition was located and converted successfully.", typeof(ProfileResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Guests are barred from auditing profile ledger segments.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No profile aggregate matched the supplied structural query identifier.")]
    public async Task<IActionResult> GetProfileById(int profileId)
    {
        var getProfileByIdQuery = new GetProfileByIdQuery(profileId);
        var profile = await profileQueryService.Handle(getProfileByIdQuery);
        if (profile is null) return NotFound();
        var profileResource = ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
        return Ok(profileResource);
    }

    /// <summary>
    ///     Creates and records a new personal profile aggregate root partition within the identity domain boundary.
    /// </summary>
    /// <param name="resource">The incoming input resource payload mapping names, contact metrics, and context required for profile initialization.</param>
    /// <returns>A created resource response alongside the tracking location parameters of the processed profile aggregate root.</returns>
    [HttpPost]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new identity profile entry", 
        Description = "Constructs and initialises a new user profile aggregate partition. Open to guests during registration and management nodes.", 
        OperationId = "CreateProfile")]
    [SwaggerResponse(StatusCodes.Status201Created, "The profile aggregate root was successfully validated, processed, and tracked.", typeof(ProfileResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided construction resource layout structure contains invalid parameters or violates business constraints.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileResource resource)
    {
        var createProfileCommand = CreateProfileCommandFromResourceAssembler.ToCommandFromResource(resource);
        var profile = await profileCommandService.Handle(createProfileCommand);
        if (profile is null) return BadRequest();
        var profileResource = ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
        return CreatedAtAction(nameof(GetProfileById), new { profileId = profile.Id }, profileResource);
    }

    /// <summary>
    ///     Retrieves an enumerable collection mapping all registered identity profile aggregate resources in the persistence subsystem.
    /// </summary>
    /// <returns>An asynchronous action result containing an enumerable collection of profile representations.</returns>
    [HttpGet]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get all tracked identity profiles", 
        Description = "Retrieves all personal profile aggregate definitions converted into view resources. Restricted exclusively to administrative clearance profiles.", 
        OperationId = "GetAllProfiles")]
    [SwaggerResponse(StatusCodes.Status200OK, "The overall profile resource inventory list was successfully fetched.", typeof(IEnumerable<ProfileResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks the administrative clearance parameters required to execute ledger enumeration.")]
    public async Task<IActionResult> GetAllProfiles()
    {
        var getAllProfilesQuery = new GetAllProfilesQuery();
        var profiles = await profileQueryService.Handle(getAllProfilesQuery);
        var profileResources = profiles.Select(ProfileResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(profileResources);
    }
}