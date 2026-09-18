using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Controllers.Authorization;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendAwSmartstay.API.Models.IoT;
using BackendAwSmartstay.API.Infrastructure.Telemetry;

namespace BackendAwSmartstay.API.Controllers;

// In-memory IoT emulator. Real route: /api/v1/io-t-emulator/... (kebab-case convention).
// Authorization (R5): roles per endpoint come from Policies; the room itself is checked with
// RoomDeviceAuthorizationHandler (guest: room of their Confirmed stay in effect today; admin/maintenance:
// rooms of their hotel; chain_admin: every room). Unknown rooms answer 404.
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class IoTEmulatorController(
    IAccommodationsContextFacade accommodationsContextFacade,
    IAuthorizationService authorizationService) : ControllerBase
{
    // POST /api/v1/io-t-emulator/rooms/{roomId}/inject-telemetry
    [HttpPost("rooms/{roomId:int:min(1)}/inject-telemetry")]
    [Authorize(Policy = Policies.InjectTelemetry)]
    public async Task<IActionResult> InjectTelemetry(int roomId, [FromBody] InjectTelemetryRequest request)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        if (request == null || string.IsNullOrWhiteSpace(request.SimulatedSensorType) || request.ReadingValue == null)
        {
            return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
        }

        IoTEmulatorStore.Update(roomId, state =>
        {
            state.LastCommandReceived = $"INJECT_{request.SimulatedSensorType}";
            
            if (request.SimulatedSensorType.ToUpper() == "TEMPERATURE_SENSOR")
            {
                if (double.TryParse(request.ReadingValue, out double parsedTemp))
                {
                    state.CurrentTemperature = parsedTemp;
                }
            }
            else if (request.SimulatedSensorType.ToUpper() == "PIR_MOTION_DETECTOR")
            {
                state.MotionDetected = request.ReadingValue.ToUpper() != "NO_MOTION_DETECTED_30MIN";
                
                // Lógica de negocio inteligente simulada: Si no hay movimiento, apaga el aire bajando la carga
                if (!state.MotionDetected)
                {
                    state.LastCommandReceived = "AUTO_POWER_SAVING_MODE_TRIGGERED";
                }
            }

            if (request.ForceStatusChange)
            {
                state.HardwareStatus = "ALTERED_BY_EMULATOR_TRIGGER";
            }
        });

        var updatedState = IoTEmulatorStore.GetOrAdd(roomId);
        return Ok(updatedState);
    }

    // POST /api/v1/io-t-emulator/rooms/{roomId}/thermostat
    [HttpPost("rooms/{roomId:int:min(1)}/thermostat")]
    [Authorize(Policy = Policies.ControlRoomDevices)]
    public async Task<IActionResult> SetThermostat(int roomId, [FromBody] SetThermostatRequest request)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        if (request == null || string.IsNullOrWhiteSpace(request.FanSpeed) || string.IsNullOrWhiteSpace(request.SimulationMode))
        {
            return BadRequest("Parámetros del termostato inválidos.");
        }

        IoTEmulatorStore.Update(roomId, state =>
        {
            state.LastCommandReceived = $"SET_TEMPERATURE_{request.TargetTemperatureCelsius}C_FAN_{request.FanSpeed.ToUpper()}";
            state.CurrentTemperature = request.TargetTemperatureCelsius;
            state.EmulatedDevice = request.SimulationMode;
            state.HardwareStatus = "OPERATIONAL_EMULATED";
        });

        var updatedState = IoTEmulatorStore.GetOrAdd(roomId);
        return Ok(updatedState);
    }

    // GET /api/v1/io-t-emulator/rooms/{roomId}/actuators-state
    [HttpGet("rooms/{roomId:int:min(1)}/actuators-state")]
    [Authorize(Policy = Policies.ReadRoomDevices)]
    public async Task<IActionResult> GetActuatorsState(int roomId)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        var state = IoTEmulatorStore.GetOrAdd(roomId);
        return Ok(state);
    }

    /// <summary>404 when the room does not exist, 403 (native Forbid) when it is outside the requester's reach.</summary>
    private async Task<IActionResult?> EnsureRoomAccessAsync(int roomId)
    {
        var hotelId = await accommodationsContextFacade.FetchHotelIdOfRoomAsync(roomId);
        if (hotelId is null) return NotFound();

        var authorization = await authorizationService.AuthorizeAsync(
            User, new DeviceRoom(roomId, hotelId.Value), RoomDeviceAccessRequirement.Instance);
        return authorization.Succeeded ? null : Forbid();
    }
}
