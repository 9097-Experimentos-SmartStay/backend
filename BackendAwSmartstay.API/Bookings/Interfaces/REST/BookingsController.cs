using System.Net.Mime;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling operational, guest, and administrative 
///     requests tracking the complete transactional life cycle of booking aggregate roots within the booking bounded context.
/// </summary>
/// <param name="bookingCommandService">The domain command service used to handle booking state transitions and mutations.</param>
/// <param name="bookingQueryService">The domain query service used to handle booking state extraction and tracking queries.</param>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Booking Endpoints")]
public class BookingsController(
    IBookingCommandService bookingCommandService,
    IBookingQueryService bookingQueryService) : ControllerBase
{
    /// <summary>
    ///     Retrieves a unique booking aggregate partition by its technical identity marker.
    /// </summary>
    /// <param name="bookingId">The structural domain identity number of the target booking aggregate root.</param>
    /// <returns>An asynchronous action result containing the matching booking resource representation state, or NotFound.</returns>
    [HttpGet("{bookingId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get booking by its unique identifier",
        Description = "Retrieves state parameters, scheduling metrics, and transactional properties for a single booking aggregate entry. Restricted to management nodes.",
        OperationId = "GetBookingById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The booking aggregate was located and converted successfully.", typeof(BookingResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Guests are barred from auditing broad reservation index segments.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No booking aggregate matched the supplied structural query identifier.")]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var getBookingByIdQuery = new GetBookingByIdQuery(bookingId);
        var booking = await bookingQueryService.Handle(getBookingByIdQuery);
        if (booking is null) return NotFound();
        var resource = BookingResourceFromEntityAssembler.ToResourceFromEntity(booking);
        return Ok(resource);
    }

    /// <summary>
    ///     Creates and records a new booking aggregate root partition within the transactional scheduling boundary.
    /// </summary>
    /// <param name="resource">The incoming resource payload mapping properties and context metrics required for booking initialization.</param>
    /// <returns>A created resource location confirmation alongside the structural tracking instance state representation.</returns>
    [HttpPost]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new reservation entry",
        Description = "Registers a new booking partition aggregate. Open to guests for self-service or staff for assisted desks.",
        OperationId = "CreateBooking")]
    [SwaggerResponse(StatusCodes.Status201Created, "The booking aggregate root was successfully validated, processed, and tracked.", typeof(BookingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided construction resource schema layout contains invalid parameters or violates business rule constraints.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingResource resource)
    {
        var createBookingCommand = CreateBookingCommandFromResourceAssembler.ToCommandFromResource(resource);
        var booking = await bookingCommandService.Handle(createBookingCommand);
        if (booking is null) return BadRequest();
        var bookingResource = BookingResourceFromEntityAssembler.ToResourceFromEntity(booking);
        return CreatedAtAction(nameof(GetBookingById), new { bookingId = booking.Id }, bookingResource);
    }
    
    /// <summary>
    ///     Retrieves an enumerable collection mapping all registered booking aggregate resources in the persistence subsystem.
    /// </summary>
    /// <returns>An asynchronous action result containing an enumerable collection of booking representations.</returns>
    [HttpGet]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get all tracked reservations",
        Description = "Retrieves all booking aggregates converted into view resources. Restricted exclusively to administrative clearance profiles.",
        OperationId = "GetAllBookings")]
    [SwaggerResponse(StatusCodes.Status200OK, "The overall booking resource inventory list was successfully fetched.", typeof(IEnumerable<BookingResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks the administrative clearance parameter to execute ledger enumeration.")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await bookingQueryService.Handle(new GetAllBookingsQuery());
        var bookingResources = bookings.Select(BookingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(bookingResources);
    }

    /// <summary>
    ///     Filters and extracts a sub-collection of active booking resources associated with a specific structural room identifier.
    /// </summary>
    /// <param name="roomId">The structural tracking domain identity marker of the target room node aggregate.</param>
    /// <returns>An enumerable resource listing matching reservation representations linked to the specific room asset node.</returns>
    [HttpGet("room/{roomId:int}")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get booking ledger histories by room node",
        Description = "Retrieves a subset of booking aggregates filtering criteria by their associated target asset mapping node.",
        OperationId = "GetBookingsByRoomId")]
    [SwaggerResponse(StatusCodes.Status200OK, "The filtered booking resource subset was successfully retrieved.", typeof(IEnumerable<BookingResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks administrative tracking authorization.")]
    public async Task<IActionResult> GetBookingsByRoomId(int roomId)
    {
        var bookings = await bookingQueryService.Handle(new GetBookingsByRoomIdQuery(roomId));
        var bookingResources = bookings.Select(BookingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(bookingResources);
    }
    
    /// <summary>
    ///     Processes operational confirmation routines, mutating the target booking state transition to Confirmed.
    /// </summary>
    /// <param name="bookingId">The unique domain identifier pointing to the aggregate instance undergoing verification state changes.</param>
    /// <returns>The newly updated booking representation state outcome resource reflecting check-in or transactional readiness.</returns>
    [HttpPost("{bookingId:int}/confirm")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Confirm an active reservation entry status",
        Description = "Mutates structural booking state properties to locked confirmation codes. Strictly for verified operational personnel.",
        OperationId = "ConfirmBooking")]
    [SwaggerResponse(StatusCodes.Status200OK, "The target booking state mutation was verified and committed successfully.", typeof(BookingResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Only cleared operational nodes may trigger inventory authorization confirmations.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted booking index node could not be pulled for status update.")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var confirmBookingCommand = new ConfirmBookingCommand(bookingId);
        var booking = await bookingCommandService.Handle(confirmBookingCommand);
        if (booking is null) return NotFound();
        var bookingResource = BookingResourceFromEntityAssembler.ToResourceFromEntity(booking);
        return Ok(bookingResource);
    }

    /// <summary>
    ///     Processes cancellation lifecycle routines, mutating the target booking state transition to Canceled and releasing assets.
    /// </summary>
    /// <param name="bookingId">The unique domain root aggregate identifier targeted for operational cancellation routines.</param>
    /// <returns>The final detached state cancellation representation resource data layout.</returns>
    [HttpPost("{bookingId:int}/cancel")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Cancel a registered reservation entry layout",
        Description = "Triggers systemic cancellation state changes for a single booking target, releasing inventory dependencies.",
        OperationId = "CancelBooking")]
    [SwaggerResponse(StatusCodes.Status200OK, "The targeted booking lifecycle index node was successfully transitioned to cancelled status.", typeof(BookingResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The requesting identity lacks clearance parameters required to alter reservation ledgers.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The targeted booking instance was not active or present within the context persistence tree.")]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var cancelBookingCommand = new CancelBookingCommand(bookingId);
        var booking = await bookingCommandService.Handle(cancelBookingCommand);
        if (booking is null) return NotFound();
        var bookingResource = BookingResourceFromEntityAssembler.ToResourceFromEntity(booking);
        return Ok(bookingResource);
    }
}