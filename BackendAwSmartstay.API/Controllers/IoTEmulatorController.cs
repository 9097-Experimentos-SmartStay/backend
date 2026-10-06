using System.Globalization;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
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
// RoomDeviceAuthorizationHandler (guest: room of their Confirmed stay in effect today; hotel staff:
// rooms of their hotel; chain_admin: every room). Unknown rooms answer 404.
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class IoTEmulatorController(
    IAccommodationsContextFacade accommodationsContextFacade,
    IAuthorizationService authorizationService) : ControllerBase
{
    private const string TemperatureSensor = "TEMPERATURE_SENSOR";
    private const string MotionSensor = "PIR_MOTION_DETECTOR";
    private const string NoMotionReading = "NO_MOTION_DETECTED_30MIN";

    // Same rules as the thermostat of the web app (US-11): 16–30 °C in half-degree steps.
    private const double MinTargetTemperature = 16;
    private const double MaxTargetTemperature = 30;
    private static readonly string[] FanSpeeds = ["Low", "Medium", "High"];

    // POST /api/v1/io-t-emulator/rooms/{roomId}/inject-telemetry
    [HttpPost("rooms/{roomId:int:min(1)}/inject-telemetry")]
    [Authorize(Policy = Policies.InjectTelemetry)]
    public async Task<IActionResult> InjectTelemetry(int roomId, [FromBody] InjectTelemetryRequest request)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        if (request == null || string.IsNullOrWhiteSpace(request.SimulatedSensorType) || string.IsNullOrWhiteSpace(request.ReadingValue))
            throw new DomainValidationException(IoTErrorCodes.TelemetryInvalid,
                "Send the simulated sensor type and the reading value.");

        var sensor = request.SimulatedSensorType.Trim().ToUpperInvariant();
        var reading = request.ReadingValue.Trim();
        if (sensor != TemperatureSensor && sensor != MotionSensor)
            throw new DomainValidationException(IoTErrorCodes.TelemetryInvalid,
                $"The simulated sensor type must be {TemperatureSensor} or {MotionSensor}.");

        // Invariant culture: "24.5" must read the same on a server configured in es-PE.
        double parsedTemp = 0;
        if (sensor == TemperatureSensor
            && !double.TryParse(reading, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedTemp))
            throw new DomainValidationException(IoTErrorCodes.TelemetryInvalid,
                "The reading of the temperature sensor must be a number.");

        var updatedState = IoTEmulatorStore.Update(roomId, state =>
        {
            state.LastCommandReceived = $"INJECT_{sensor}";

            if (sensor == TemperatureSensor)
            {
                state.CurrentTemperature = parsedTemp;
            }
            else
            {
                state.MotionDetected = !reading.Equals(NoMotionReading, StringComparison.OrdinalIgnoreCase);

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

        return Ok(updatedState);
    }

    // POST /api/v1/io-t-emulator/rooms/{roomId}/thermostat
    [HttpPost("rooms/{roomId:int:min(1)}/thermostat")]
    [Authorize(Policy = Policies.ControlRoomDevices)]
    public async Task<IActionResult> SetThermostat(int roomId, [FromBody] SetThermostatRequest request)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        if (request == null || string.IsNullOrWhiteSpace(request.FanSpeed))
            throw new DomainValidationException(IoTErrorCodes.ThermostatInvalid,
                "Send the target temperature and the fan speed of the thermostat.");

        var target = request.TargetTemperatureCelsius;
        if (!double.IsFinite(target) || target < MinTargetTemperature || target > MaxTargetTemperature
            || Math.Round(target * 2) != target * 2)
            throw new DomainValidationException(IoTErrorCodes.ThermostatInvalid,
                $"The target temperature must be between {MinTargetTemperature} and {MaxTargetTemperature} °C, in half-degree steps.");

        var fanSpeed = FanSpeeds.FirstOrDefault(speed => speed.Equals(request.FanSpeed.Trim(), StringComparison.OrdinalIgnoreCase));
        if (fanSpeed is null)
            throw new DomainValidationException(IoTErrorCodes.ThermostatInvalid,
                "The fan speed must be Low, Medium or High.");

        var updatedState = IoTEmulatorStore.Update(roomId, state =>
        {
            state.LastCommandReceived = string.Create(CultureInfo.InvariantCulture,
                $"SET_TEMPERATURE_{target}C_FAN_{fanSpeed.ToUpperInvariant()}");
            state.CurrentTemperature = target;
            if (!string.IsNullOrWhiteSpace(request.SimulationMode))
                state.EmulatedDevice = request.SimulationMode.Trim();
            // HardwareStatus is left as is: a thermostat command does not repair an altered board.
        });

        return Ok(updatedState);
    }

    // GET /api/v1/io-t-emulator/rooms/{roomId}/actuators-state
    [HttpGet("rooms/{roomId:int:min(1)}/actuators-state")]
    [Authorize(Policy = Policies.ReadRoomDevices)]
    public async Task<IActionResult> GetActuatorsState(int roomId)
    {
        if (await EnsureRoomAccessAsync(roomId) is { } denied) return denied;

        return Ok(IoTEmulatorStore.GetOrAdd(roomId));
    }

    /// <summary>
    ///     404 <c>room.not_found</c> when the room does not exist (the code tells it apart from a missing route),
    ///     403 (native Forbid) when it is outside the requester's reach.
    /// </summary>
    private async Task<IActionResult?> EnsureRoomAccessAsync(int roomId)
    {
        var hotelId = await accommodationsContextFacade.FetchHotelIdOfRoomAsync(roomId)
                      ?? throw new EntityNotFoundException("Room", roomId);

        var authorization = await authorizationService.AuthorizeAsync(
            User, new DeviceRoom(roomId, hotelId), RoomDeviceAccessRequirement.Instance);
        return authorization.Succeeded ? null : Forbid();
    }
}
