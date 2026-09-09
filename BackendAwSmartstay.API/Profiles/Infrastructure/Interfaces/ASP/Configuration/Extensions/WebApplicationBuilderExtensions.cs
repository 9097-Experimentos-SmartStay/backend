using BackendAwSmartstay.API.Profiles.Application.ACL;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Services;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
/// Provides extension methods for configuring Profile context services in the application.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers all Profile-related services into the dependency injection container.
    /// </summary>
    public static void AddProfilesContextServices(this WebApplicationBuilder builder)
    {
        // Repositories
        builder.Services.AddScoped<IGuestProfileRepository, GuestProfileRepository>();
        builder.Services.AddScoped<IStaffProfileRepository, StaffProfileRepository>();

        // Domain & Outbound Services
        builder.Services.AddScoped<IEmployeeCodeGenerator, EmployeeCodeGenerator>();
        builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Guest Application Services
        builder.Services.AddScoped<IGuestProfileCommandService, GuestProfileCommandService>();
        builder.Services.AddScoped<IGuestProfileQueryService, GuestProfileQueryService>();

        // Staff Application Services
        builder.Services.AddScoped<IStaffProfileCommandService, StaffProfileCommandService>();
        builder.Services.AddScoped<IStaffProfileQueryService, StaffProfileQueryService>();

        // ACL Facades
        builder.Services.AddScoped<IGuestProfilesContextFacade, GuestProfilesContextFacade>();
        builder.Services.AddScoped<IStaffProfilesContextFacade, StaffProfilesContextFacade>();
    }
}