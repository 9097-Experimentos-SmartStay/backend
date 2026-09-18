using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using System.Net.Mime;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Transform;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST;

/// <summary>
/// Inbound REST adapter for Guest Profiles operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Guest Profiles Endpoints.")]
public class GuestsController(
    IGuestProfileCommandService guestCommandService,
    IGuestProfileQueryService guestQueryService)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception, UserRoles.Guest)]
    [SwaggerOperation(Summary = "Get guest profile by ID", OperationId = "GetGuestProfileById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile found.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGuestProfileByIdQuery(new GuestProfileId(id));
        var guest = await guestQueryService.Handle(query);
        if (guest is null || !CanAccess(guest.UserId)) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpGet("email/{email}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception)]
    [SwaggerOperation(Summary = "Get guest profile by email address", OperationId = "GetGuestProfileByEmail")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile found.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var query = new GetGuestProfileByEmailQuery(new EmailAddress(email));
        var guest = await guestQueryService.Handle(query);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpGet("user/{userId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception, UserRoles.Guest)]
    [SwaggerOperation(Summary = "Get guest profile by IAM User ID", OperationId = "GetGuestProfileByUserId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile found.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        // Guests can only look up their own profile
        if (CallerGuestUserId() is { } guestUserId && guestUserId != userId) return NotFound();

        var query = new GetGuestProfileByUserIdQuery(new UserId(userId));
        var guest = await guestQueryService.Handle(query);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpGet]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception)]
    [SwaggerOperation(Summary = "Get all guest profiles", OperationId = "GetAllGuestProfiles")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profiles retrieved.", typeof(IEnumerable<GuestProfileResource>))]
    public async Task<IActionResult> GetAll()
    {
        var guests = await guestQueryService.Handle(new GetAllGuestProfilesQuery());
        return Ok(guests.Select(GuestResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception, UserRoles.Guest)]
    [SwaggerOperation(Summary = "Create a new guest profile", OperationId = "CreateGuestProfile")]
    [SwaggerResponse(StatusCodes.Status201Created, "Guest profile created successfully.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input data.")]
    public async Task<IActionResult> Create([FromBody] CreateGuestProfileResource resource)
    {
        // A guest can only create their own profile: the user link comes from the token
        if (CallerGuestUserId() is { } guestUserId) resource = resource with { UserId = guestUserId };

        var command = GuestResourceAssembler.ToCommandFromResource(resource);
        var guest = await guestCommandService.Handle(command);
        if (guest is null) return BadRequest();
        var response = GuestResourceAssembler.ToResourceFromEntity(guest);
        return CreatedAtAction(nameof(GetById), new { id = guest.Id.Value }, response);
    }

    [HttpPost("{id:guid}/link-user")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Guest)]
    [SwaggerOperation(Summary = "Link guest profile to an IAM user account", OperationId = "LinkGuestToUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile linked to user.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> LinkToUser(Guid id, [FromBody] LinkGuestToUserResource resource)
    {
        // A guest can only link a profile to their own account
        if (CallerGuestUserId() is { } guestUserId && guestUserId != resource.UserId)
            throw new OperationNotAllowedException("Guests can only link a profile to their own user account.");

        var command = new LinkGuestToUserCommand(
            new GuestProfileId(id),
            new UserId(resource.UserId),
            new EmailAddress(resource.VerifiedEmail));

        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpPut("{id:guid}/contact-info")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception, UserRoles.Guest)]
    [SwaggerOperation(Summary = "Update guest contact information", OperationId = "UpdateGuestContactInfo")]
    [SwaggerResponse(StatusCodes.Status200OK, "Contact information updated.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> UpdateContactInfo(Guid id, [FromBody] UpdateGuestContactInformationResource resource)
    {
        if (CallerGuestUserId() is not null)
        {
            var current = await guestQueryService.Handle(new GetGuestProfileByIdQuery(new GuestProfileId(id)));
            if (current is null || !CanAccess(current.UserId)) return NotFound();
        }

        var address = resource.Street != null && resource.Number != null && resource.City != null && resource.PostalCode != null && resource.Country != null
            ? new StreetAddress(resource.Street, resource.Number, resource.City, resource.PostalCode, resource.Country)
            : null;

        var command = new UpdateGuestContactInformationCommand(
            new GuestProfileId(id),
            new PhoneNumber(resource.Phone),
            address);

        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpPut("{id:guid}/identification")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception)]
    [SwaggerOperation(Summary = "Set initial guest identification document", OperationId = "SetGuestIdentification")]
    [SwaggerResponse(StatusCodes.Status200OK, "Identification document set.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> SetIdentification(Guid id, [FromBody] SetGuestIdentificationResource resource)
    {
        var command = new SetGuestIdentificationCommand(
            new GuestProfileId(id),
            new IdentificationDocument(resource.DocumentType, resource.DocumentNumber));

        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpPost("{id:guid}/correct-identification")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(Summary = "Correct guest identification document with audit trail justification", OperationId = "CorrectGuestIdentification")]
    [SwaggerResponse(StatusCodes.Status200OK, "Identification document corrected.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> CorrectIdentification(Guid id, [FromBody] CorrectGuestIdentificationResource resource)
    {
        var command = new CorrectGuestIdentificationCommand(
            new GuestProfileId(id),
            new IdentificationDocument(resource.NewDocumentType, resource.NewDocumentNumber),
            resource.Reason,
            // Audit identity comes from the token, not from the body
            new UserId(HttpContext.RequireAuthenticatedUser().Id));

        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(Summary = "Deactivate guest profile", OperationId = "DeactivateGuestProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile deactivated.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new DeactivateGuestProfileCommand(new GuestProfileId(id));
        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(Summary = "Activate guest profile", OperationId = "ActivateGuestProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "Guest profile activated.", typeof(GuestProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Guest profile not found.")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var command = new ActivateGuestProfileCommand(new GuestProfileId(id));
        var guest = await guestCommandService.Handle(command);
        if (guest is null) return NotFound();
        return Ok(GuestResourceAssembler.ToResourceFromEntity(guest));
    }

    /// <summary>The caller's user id when the caller is a guest; null for hotel staff roles.</summary>
    private int? CallerGuestUserId()
    {
        var actor = HttpContext.RequireAuthenticatedUser();
        return actor.IsInRole(UserRoles.Guest) ? actor.Id : null;
    }

    /// <summary>Staff roles can access any profile; a guest only the profile linked to their account.</summary>
    private bool CanAccess(UserId? profileUserId) =>
        CallerGuestUserId() is not { } guestUserId || profileUserId?.Value == guestUserId;
}
