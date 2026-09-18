using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Authorization;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling corporate and guest operations 
///     related to hotel property aggregates within the hotel accommodation bounded context.
/// </summary>
[Authorize(Policy = Policies.ReadInventory)]
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Available Hotel Endpoints")]
public class HotelsController(
    IHotelCommandService hotelCommandService,
    IHotelQueryService hotelQueryService,
    IAuthorizationService authorizationService) : ControllerBase
{
    /// <summary>
    ///     Retrieves a collection of all registered hotel property resources.
    /// </summary>
    /// <remarks>
    ///     Accessible by all verified roles (Guests, Admins, and ChainAdmins) across client applications.
    /// </remarks>
    /// <returns>An asynchronous action result containing an enumerable collection of hotel representations.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all hotels",
        Description = "Retrieves all hotel aggregates mapped to external representations. Open to guests and staff.",
        OperationId = "GetAllHotels")]
    [SwaggerResponse(StatusCodes.Status200OK, "The hotel resource list was successfully fetched.", typeof(IEnumerable<HotelResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetAllHotels()
    {
        var hotels = await hotelQueryService.Handle(new GetAllHotelsQuery());
        var resources = hotels.Select(HotelResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    ///     Retrieves a unique hotel property resource by its technical identity marker.
    /// </summary>
    /// <param name="hotelId">The structural domain identity number of the hotel target aggregate.</param>
    /// <returns>The matching hotel representation resource context, or NotFound.</returns>
    [HttpGet("{hotelId:int}")]
    [SwaggerOperation(
        Summary = "Get hotel by its unique identifier",
        Description = "Retrieves structural property details for a single hotel aggregate from its domain identifier.",
        OperationId = "GetHotelById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The hotel aggregate was located and mapped successfully.", typeof(HotelResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No hotel aggregate matched the supplied structural query identifier.")]
    public async Task<IActionResult> GetHotelById(int hotelId)
    {
        var hotel = await hotelQueryService.Handle(new GetHotelByIdQuery(hotelId));
        if (hotel is null) return NotFound();
        var resource = HotelResourceFromEntityAssembler.ToResourceFromEntity(hotel);
        return Ok(resource);
    }

    /// <summary>
    ///     Creates a new hotel aggregate root inside the transactional bounded context.
    /// </summary>
    /// <param name="resource">The incoming payload representation mapping properties required for construction.</param>
    /// <returns>A created resource location confirmation with the persistence tracking instance representation.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.ManageHotels)]
    [SwaggerOperation(
        Summary = "Create a new hotel property entry",
        Description = "Constructs a new hotel aggregate root. Restricted exclusively to administrative and corporate management roles. When a hotel administrator registers their (single) hotel it becomes their hotelId and their sessions end (401 auth.session_revoked, reason assignment_changed): they sign in again to manage it.",
        OperationId = "CreateHotel")]
    [SwaggerResponse(StatusCodes.Status201Created, "The hotel aggregate root was successfully created and tracked.", typeof(HotelResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided construction resource structure contains invalid constraints.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Only Admin or ChainAdmin operators are cleared to execute infrastructure initialization.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A hotel administrator already has a hotel (an admin registers only their own hotel).")]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelResource resource)
    {
        var registrant = new HotelRegistrant(User.GetUserId(), User.IsChainAdmin(), User.GetHotelId());
        var command = CreateHotelCommandFromResourceAssembler.ToCommandFromResource(resource, registrant);
        var hotel = await hotelCommandService.Handle(command);
        
        if (hotel is null) return BadRequest();
        
        var hotelResource = HotelResourceFromEntityAssembler.ToResourceFromEntity(hotel);
        return CreatedAtAction(nameof(GetHotelById), new { hotelId = hotel.Id }, hotelResource);
    }
    
    /// <summary>
    ///     Updates the state representation parameters of a registered hotel aggregate root.
    /// </summary>
    /// <param name="hotelId">The structural identifier pointing to the aggregate instance undergoing mutation.</param>
    /// <param name="resource">The incoming state modification layout resource constraints.</param>
    /// <returns>The updated hotel resource state outcome representation.</returns>
    [HttpPut("{hotelId:int}")]
    [Authorize(Policy = Policies.ManageHotels)]
    [SwaggerOperation(
        Summary = "Update an existing hotel aggregate's context properties",
        Description = "Mutates descriptive fields on an active hotel target. Only accessible by authorized management nodes.",
        OperationId = "UpdateHotel")]
    [SwaggerResponse(StatusCodes.Status200OK, "The hotel aggregate state was updated successfully.", typeof(HotelResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient administrative privilege levels.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted hotel aggregate could not be extracted for state alteration.")]
    public async Task<IActionResult> UpdateHotel(int hotelId, [FromBody] UpdateHotelResource resource)
    {
        var notAllowed = await EnsureCanManageHotelAsync(hotelId);
        if (notAllowed is not null) return notAllowed;

        var command = UpdateHotelCommandFromResourceAssembler.ToCommandFromResource(hotelId, resource);
        var updatedHotel = await hotelCommandService.Handle(command);

        if (updatedHotel is null) return NotFound();

        var hotelResource = HotelResourceFromEntityAssembler.ToResourceFromEntity(updatedHotel);
        return Ok(hotelResource);
    }

    /// <summary>
    ///     Removes an active hotel property aggregate from the domain persistence subsystem.
    /// </summary>
    /// <param name="hotelId">The unique structural aggregate identifier targeted for transactional removal.</param>
    /// <returns>The final detached state representation of the processed resource entry.</returns>
    [HttpDelete("{hotelId:int}")]
    [Authorize(Policy = Policies.ManageHotels)]
    [SwaggerOperation(
        Summary = "Delete a hotel property cluster",
        Description = "Triggers complete cascading teardown routines for a single hotel entity group. Strictly for administrative clearance nodes.",
        OperationId = "DeleteHotel")]
    [SwaggerResponse(StatusCodes.Status200OK, "The hotel property aggregate hierarchy was completely processed and decommissioned.", typeof(HotelResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks the extreme corporate validation depth to delete property nodes.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted hotel index node was not present in the structural cluster tree.")]
    public async Task<IActionResult> DeleteHotel(int hotelId)
    {
        var notAllowed = await EnsureCanManageHotelAsync(hotelId);
        if (notAllowed is not null) return notAllowed;

        var command = new DeleteHotelCommand(hotelId);
        var deletedHotel = await hotelCommandService.Handle(command);

        if (deletedHotel is null) return NotFound();

        var hotelResource = HotelResourceFromEntityAssembler.ToResourceFromEntity(deletedHotel);
        return Ok(hotelResource);
    }

    /// <summary>
    ///     Resource-based authorization: 404 when the hotel does not exist, 403 (native Forbid) when it is
    ///     outside the requester's scope, null when the requester may manage it.
    /// </summary>
    private async Task<IActionResult?> EnsureCanManageHotelAsync(int hotelId)
    {
        var hotel = await hotelQueryService.Handle(new GetHotelByIdQuery(hotelId));
        if (hotel is null) return NotFound();

        var authorization = await authorizationService.AuthorizeAsync(User, hotel, HotelManagementRequirement.Instance);
        return authorization.Succeeded ? null : Forbid();
    }
}
