namespace BackendAwSmartstay.API.Models.IoT;

// DTO para inyección de telemetría (Simulación de entrada de sensor)
public record InjectTelemetryRequest(
    string SimulatedSensorType, // Ejemplo: "PIR_MOTION_DETECTOR", "TEMPERATURE_SENSOR"
    string ReadingValue,        // Ejemplo: "NO_MOTION_DETECTED_30MIN", "24.5"
    bool ForceStatusChange
);

// DTO para el control del termostato por parte del huésped
public record SetThermostatRequest(
    double TargetTemperatureCelsius,
    string FanSpeed, // "Low", "Medium", "High"
    string SimulationMode = "CiscoPacketTracer_SBC"
);

// Modelo que representa el estado virtual de la habitación en memoria
public class EmulatedRoomState
{
    public int RoomId { get; set; }
    public string EmulatedDevice { get; set; } = "Cisco_SBC_Board_Virtual";
    public double CurrentTemperature { get; set; } = 22.0;
    public bool MotionDetected { get; set; } = true;
    public string LastCommandReceived { get; set; } = "INITIALIZE_SYSTEM";
    public string HardwareStatus { get; set; } = "OPERATIONAL_EMULATED";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}