using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST;

/// <summary>Room availability for a stay (US-29 scenario 1), answered by the Bookings context.</summary>
[ApiController]
[Route("api/v1/rooms")]
[Produces("application/json")]
[Authorize(Policy = Policies.ReadInventory)]
[Tags("Rooms")]
public class RoomAvailabilityController(IRoomAvailabilityQueryService roomAvailabilityQueryService) : ControllerBase
{
    /// <summary>Lists the rooms free for the whole stay, with price per night and total price.</summary>
    /// <remarks>
    ///     A room is free when it is not under maintenance and no Pending or Confirmed booking shares a night with
    ///     the stay (back-to-back stays are allowed). Sorted by hotel, then price.
    /// </remarks>
    /// <param name="checkIn">Check-in date (yyyy-MM-dd).</param>
    /// <param name="checkOut">Check-out date (yyyy-MM-dd), at least one day after check-in.</param>
    /// <param name="hotelId">Only rooms of this hotel (all hotels when omitted).</param>
    [HttpGet("available")]
    [SwaggerOperation(Summary = "Get available rooms for a stay", OperationId = "GetAvailableRooms")]
    [ProducesResponseType(typeof(IEnumerable<AvailableRoomResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableRooms(
        [FromQuery, BindRequired] DateOnly checkIn,
        [FromQuery, BindRequired] DateOnly checkOut,
        [FromQuery] int? hotelId)
    {
        var dates = new DateRange(checkIn.ToDateTime(TimeOnly.MinValue), checkOut.ToDateTime(TimeOnly.MinValue));
        var rooms = await roomAvailabilityQueryService.Handle(new GetAvailableRoomsQuery(hotelId, dates));
        return Ok(rooms.Select(available => new AvailableRoomResource(
            available.Room.RoomId, available.Room.HotelId, available.Room.RoomTypeId, available.Room.RoomTypeName,
            available.Room.Description, available.Room.Amenities, available.Room.Status, true,
            available.Room.PricePerNight, available.Nights, available.TotalPrice, checkIn, checkOut)));
    }
}
