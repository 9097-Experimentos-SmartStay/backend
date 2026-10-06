using System.Collections.Concurrent;
using BackendAwSmartstay.API.Models.IoT;

namespace BackendAwSmartstay.API.Infrastructure.Telemetry;

public static class IoTEmulatorStore
{
    // Almacena el estado por cada número de habitación (RoomId)

    private static readonly ConcurrentDictionary<int, EmulatedRoomState> _roomStates = new();

    private static EmulatedRoomState StateOf(int roomId) =>
        _roomStates.GetOrAdd(roomId, id => new EmulatedRoomState { RoomId = id });

    /// <summary>Snapshot of the room's state (created with the defaults the first time it is asked for).</summary>
    public static EmulatedRoomState GetOrAdd(int roomId)
    {
        var state = StateOf(roomId);
        lock (state) return state.Snapshot();
    }

    /// <summary>Applies the change under the room's lock and returns the resulting snapshot.</summary>
    public static EmulatedRoomState Update(int roomId, Action<EmulatedRoomState> updateAction)
    {
        var state = StateOf(roomId);
        lock (state)
        {
            updateAction(state);
            state.Timestamp = DateTime.UtcNow;
            return state.Snapshot();
        }
    }
}
