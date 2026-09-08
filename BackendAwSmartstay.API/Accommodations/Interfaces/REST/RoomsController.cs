using System.Net.Mime;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
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
///     requests related to individual room aggregate roots within the accommodation bounded context.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Room Endpoints")]
public class RoomsController(
    IRoomCommandService roomCommandService,
    IRoomQueryService roomQueryService) : ControllerBase
{
    /// <summary>
    ///     Retrieves a single room resource partition by its structural domain identity marker.
    /// </summary>
    /// <param name="roomId">The unique domain identifier value representing the targeted room aggregate root.</param>
    /// <returns>An asynchronous action result containing the matching room resource state representation.</returns>
    [HttpGet("{roomId:int}")]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get room by its unique identifier",
        Description = "Retrieves state parameters and specifications for a single room aggregate entry.",
        OperationId = "GetRoomById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room aggregate was located and converted successfully.", typeof(RoomResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No room aggregate matched the supplied structural identifier.")]
    public async Task<IActionResult> GetRoomById(int roomId)
    {
        var getRoomByIdQuery = new GetRoomByIdQuery(roomId);
        var room = await roomQueryService.Handle(getRoomByIdQuery);
        if (room is null) return NotFound();
        var resource = RoomResourceFromEntityAssembler.ToResourceFromEntity(room);
        return Ok(resource);
    }

    /// <summary>
    ///     Creates a new room aggregate root inside the property tracking persistence subsystem.
    /// </summary>
    /// <param name="resource">The incoming input resource containing constraints and associations required for construction.</param>
    /// <returns>A created resource response alongside the tracking location parameters of the processed aggregate.</returns>
    [HttpPost]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new room entry",
        Description = "Registers a new room aggregate root within an existing property context. Restricted to management nodes.",
        OperationId = "CreateRoom")]
    [SwaggerResponse(StatusCodes.Status201Created, "The room aggregate root was successfully processed and initialized.", typeof(RoomResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided construction resource layout contains invalid fields or broken constraints.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Only Admin or ChainAdmin entities are cleared to mutate property assets.")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomResource resource)
    {
        var createRoomCommand = CreateRoomCommandFromResourceAssembler.ToCommandFromResource(resource);
        var room = await roomCommandService.Handle(createRoomCommand);
        if (room is null) return BadRequest();
        var roomResource = RoomResourceFromEntityAssembler.ToResourceFromEntity(room);
        return CreatedAtAction(nameof(GetRoomById), new { roomId = room.Id }, roomResource);
    }

    /// <summary>
    ///     Retrieves an enumerable collection of all active room aggregate resources.
    /// </summary>
    /// <returns>A resource collection mapping all room aggregates present in the persistent tier.</returns>
    [HttpGet]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get all registered rooms",
        Description = "Retrieves all room aggregate node instances across properties and transforms them into view resources.",
        OperationId = "GetAllRooms")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room resource list was fetched successfully.", typeof(IEnumerable<RoomResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await roomQueryService.Handle(new GetAllRoomsQuery());
        var roomResources = rooms.Select(RoomResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(roomResources);
    }
    
    /// <summary>
    ///     Filters and extracts a sub-collection of room resources associated with a specific structural type identifier.
    /// </summary>
    /// <param name="roomTypeId">The tracking domain identity marker of the target room type entity.</param>
    /// <returns>An enumerable resource listing matching room representations.</returns>
    [HttpGet("type/{roomTypeId:int}")]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get rooms by their category or room type",
        Description = "Retrieves a sub-set of room aggregates filtering criteria by their associated category index mapping.",
        OperationId = "GetRoomsByType")]
    [SwaggerResponse(StatusCodes.Status200OK, "The filtered room resource sub-list was fetched successfully.", typeof(IEnumerable<RoomResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetRoomsByType(int roomTypeId)
    {
        var rooms = await roomQueryService.Handle(new GetRoomsByTypeQuery(roomTypeId));
        var roomResources = rooms.Select(RoomResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(roomResources);
    }
    
    /// <summary>
    ///     Updates the internal state representation details of an active room aggregate root.
    /// </summary>
    /// <param name="roomId">The tracking aggregate key pointing to the target room undergoing mutation.</param>
    /// <param name="resource">The incoming state modification layout resource payload.</param>
    /// <returns>The newly updated room representation layout outcome.</returns>
    [HttpPut("{roomId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Update an existing room aggregate's context properties",
        Description = "Mutates operational values and parameters on an active room instance. Restricted to verified corporate accounts.",
        OperationId = "UpdateRoom")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room aggregate state was updated successfully.", typeof(RoomResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks administrative authorization parameters.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted room aggregate node could not be pulled for state alteration.")]
    public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] UpdateRoomResource resource)
    {
        var command = UpdateRoomCommandFromResourceAssembler.ToCommandFromResource(roomId, resource);
        var updatedRoom = await roomCommandService.Handle(command);

        if (updatedRoom is null) return NotFound();

        var roomResource = RoomResourceFromEntityAssembler.ToResourceFromEntity(updatedRoom);
        return Ok(roomResource);
    }

    /// <summary>
    ///     Deletes an active room aggregate root tracking node from the transaction persistence engine.
    /// </summary>
    /// <param name="roomId">The unique domain root aggregate identifier targeted for operational removal.</param>
    /// <returns>The final detached state representation data layout of the processed room entry node.</returns>
    [HttpDelete("{roomId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Delete a room entity entry",
        Description = "Triggers complete structural teardown processing for a single room target aggregate. Requires full administrative clearance.",
        OperationId = "DeleteRoom")]
    [SwaggerResponse(StatusCodes.Status200OK, "The room aggregate instance was successfully cleared and decommissioned from the asset cluster.", typeof(RoomResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting entity lacks the high administrative clearance levels required to execute asset deletion.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted room asset node was not present in the structural system cluster tree.")]
    public async Task<IActionResult> DeleteRoom(int roomId)
    {
        var command = new DeleteRoomCommand(roomId);
        var deletedRoom = await roomCommandService.Handle(command);

        if (deletedRoom is null) return NotFound();

        var roomResource = RoomResourceFromEntityAssembler.ToResourceFromEntity(deletedRoom);
        return Ok(roomResource);
    }
}