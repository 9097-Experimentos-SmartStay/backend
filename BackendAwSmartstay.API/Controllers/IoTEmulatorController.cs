using Microsoft.AspNetCore.Mvc;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using BackendAwSmartstay.API.Models.IoT;
using BackendAwSmartstay.API.Infrastructure.Telemetry;

namespace BackendAwSmartstay.API.Controllers;

// In-memory IoT emulator. Real route: /api/v1/io-t-emulator/... (kebab-case convention).
// Authorization:
//   - GET actuators-state: any hotel staff (admin, chain_admin, staff, reception, housekeeping, maintenance)
//   - POST thermostat: admin, chain_admin, reception, maintenance
//   - POST inject-telemetry (simulated sensor input): admin, chain_admin, maintenance
// Guests are excluded because rooms are not linked to guests yet (no ownership check possible).
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class IoTEmulatorController : ControllerBase
{
    // POST /api/v1/io-t-emulator/rooms/{roomId}/inject-telemetry
    [HttpPost("rooms/{roomId:int:min(1)}/inject-telemetry")]
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Maintenance)]
    public IActionResult InjectTelemetry(int roomId, [FromBody] InjectTelemetryRequest request)
    {
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
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Reception, UserRoles.Maintenance)]
    public IActionResult SetThermostat(int roomId, [FromBody] SetThermostatRequest request)
    {
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
    [Authorize(UserRoles.Admin, UserRoles.ChainAdmin, UserRoles.Staff, UserRoles.Reception,
        UserRoles.Housekeeping, UserRoles.Maintenance)]
    public IActionResult GetActuatorsState(int roomId)
    {
        var state = IoTEmulatorStore.GetOrAdd(roomId);
        return Ok(state);
    }
}