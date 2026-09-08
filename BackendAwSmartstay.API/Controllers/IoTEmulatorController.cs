using Microsoft.AspNetCore.Mvc;
using BackendAwSmartstay.API.Models.IoT;
using BackendAwSmartstay.API.Infrastructure.Telemetry;

namespace BackendAwSmartstay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IoTEmulatorController : ControllerBase
{
    // POST /api/v1/iotemulator/rooms/{roomId}/inject-telemetry
    [HttpPost("rooms/{roomId:int}/inject-telemetry")]
    public IActionResult InjectTelemetry(int roomId, [FromBody] InjectTelemetryRequest request)
    {
        if (request == null)
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

    // POST /api/v1/iotemulator/rooms/{roomId}/thermostat
    [HttpPost("rooms/{roomId:int}/thermostat")]
    public IActionResult SetThermostat(int roomId, [FromBody] SetThermostatRequest request)
    {
        if (request == null)
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

    // GET /api/v1/iotemulator/rooms/{roomId}/actuators-state
    [HttpGet("rooms/{roomId:int}/actuators-state")]
    public IActionResult GetActuatorsState(int roomId)
    {
        var state = IoTEmulatorStore.GetOrAdd(roomId);
        return Ok(state);
    }
}