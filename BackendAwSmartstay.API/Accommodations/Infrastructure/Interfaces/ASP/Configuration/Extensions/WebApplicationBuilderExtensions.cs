using BackendAwSmartstay.API.Accommodations.Application.ACL;
using BackendAwSmartstay.API.Accommodations.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Accommodations.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddAccommodationsContextServices(this WebApplicationBuilder builder)
    {
        // Accommodations Bounded Context Injection Configuration

        // Repositories
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();
        builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        builder.Services.AddScoped<IHotelRepository, HotelRepository>();

        // Command Services
        builder.Services.AddScoped<IRoomCommandService, RoomCommandService>();
        builder.Services.AddScoped<IRoomTypeCommandService, RoomTypeCommandService>();
        builder.Services.AddScoped<IHotelCommandService, HotelCommandService>();

        // Query Services
        builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();
        builder.Services.AddScoped<IRoomTypeQueryService, RoomTypeQueryService>();
        builder.Services.AddScoped<IHotelQueryService, HotelQueryService>();

        // ACL Facade
        builder.Services.AddScoped<IAccommodationsContextFacade, AccommodationsContextFacade>();

        // Resource-based authorization (hotel scope)
        builder.Services.AddSingleton<IAuthorizationHandler, HotelManagementAuthorizationHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, RoomOperationsAuthorizationHandler>();
    }
}

