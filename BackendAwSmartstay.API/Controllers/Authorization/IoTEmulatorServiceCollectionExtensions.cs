using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackendAwSmartstay.API.Controllers.Authorization;

public static class IoTEmulatorServiceCollectionExtensions
{
    /// <summary>Registers the IoT emulator's resource-based authorization (R5).</summary>
    public static void AddIoTEmulatorServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(TimeProvider.System);
        // Scoped: it queries the Bookings context through its ACL facade.
        builder.Services.AddScoped<IAuthorizationHandler, RoomDeviceAuthorizationHandler>();
    }
}
