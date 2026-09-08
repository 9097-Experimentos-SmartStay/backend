using System.Net.Mime;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling operational, guest, and administrative 
///     requests related to room type categories within the accommodation bounded context.
/// </summary>
/// <param name="roomTypeCommandService">The domain command service used to handle room type mutations.</param>
/// <param name="roomTypeQueryService">The domain query service used to handle room type state extraction.</param>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Room Type Endpoints")]
public class RoomTypesController(
    IRoomTypeCommandService roomTypeCommandService,
    IRoomTypeQueryService roomTypeQueryService) : ControllerBase
{
    /// <summary>
    ///     Retrieves a single room type resource partition by its structural domain identity marker.
    /// </summary>
    /// <param name="roomTypeId">The unique domain identifier value representing the targeted room type entity.</param>
    /// <returns>An asynchronous action result containing the matching room type resource state representation.</returns>
    [HttpGet("{roomTypeId:int}")]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get room type by its unique identifier",
        Description = "Retrieves state parameters, names, and specifications for a single room type category.",
        OperationId = "GetRoomTypeById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room type category was located and converted successfully.", typeof(RoomTypeResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No room type aggregate matched the supplied structural identifier.")]
    public async Task<IActionResult> GetRoomTypeById(int roomTypeId)
    {
        var getRoomTypeByIdQuery = new GetRoomTypeByIdQuery(roomTypeId);
        var roomType = await roomTypeQueryService.Handle(getRoomTypeByIdQuery);
        if (roomType is null) return NotFound();
        var resource = RoomTypeResourceFromEntityAssembler.ToResourceFromEntity(roomType);
        return Ok(resource);
    }
    
    /// <summary>
    ///     Creates a new room type category entry inside the property catalog persistence subsystem.
    /// </summary>
    /// <param name="resource">The incoming input resource containing constraints required for room type construction.</param>
    /// <returns>A created resource response alongside the tracking location parameters of the processed entity.</returns>
    [HttpPost]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new room type category",
        Description = "Registers a new room type scheme within the catalog context. Restricted to management nodes.",
        OperationId = "CreateRoomType")]
    [SwaggerResponse(StatusCodes.Status201Created, "The room type entry was successfully processed and initialized.", typeof(RoomTypeResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided construction resource layout contains invalid fields or broken constraints.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Only Admin or ChainAdmin entities are cleared to mutate structural catalog parameters.")]
    public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeResource resource)
    {
        var createRoomTypeCommand = CreateRoomTypeCommandFromResourceAssembler.ToCommandFromResource(resource);
        var roomType = await roomTypeCommandService.Handle(createRoomTypeCommand);
        if (roomType is null) return BadRequest();
        var roomTypeResource = RoomTypeResourceFromEntityAssembler.ToResourceFromEntity(roomType);
        return CreatedAtAction(nameof(GetRoomTypeById), new { roomTypeId = roomType.Id }, roomTypeResource);
    }
    
    /// <summary>
    ///     Retrieves an enumerable collection of all registered room type resources.
    /// </summary>
    /// <returns>A resource collection mapping all room types present in the persistent tier catalog.</returns>
    [HttpGet]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get all registered room types",
        Description = "Retrieves all room type catalog definitions and transforms them into view resources.",
        OperationId = "GetAllRoomTypes")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room type resource list was fetched successfully.", typeof(IEnumerable<RoomTypeResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetAllRoomTypes()
    {
        var roomTypes = await roomTypeQueryService.Handle(new GetAllRoomTypesQuery());
        var roomTypeResources = roomTypes.Select(RoomTypeResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(roomTypeResources);
    }
}