using BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Mediator.Cortex.Configuration.Extensions;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Infrastructure;

public class ProfilesDependencyInjectionTests
{
    [Fact]
    public void AddProfilesContextServices_ShouldRegisterAllRequiredProfilesServices()
    {
        var builder = WebApplication.CreateBuilder();

        // Register AppDbContext with InMemory for test DI resolution
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.AddCortexMediatorServices();

        // Register Accommodations & Profiles services
        builder.AddAccommodationsContextServices();
        builder.AddProfilesContextServices();

        var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Repositories
        sp.GetService<IGuestProfileRepository>().Should().NotBeNull();
        sp.GetService<IStaffProfileRepository>().Should().NotBeNull();

        // Domain / Outbound Services
        sp.GetService<IEmployeeCodeGenerator>().Should().NotBeNull();
        sp.GetService<IDomainEventPublisher>().Should().NotBeNull();

        // Command Services
        sp.GetService<IGuestProfileCommandService>().Should().NotBeNull();
        sp.GetService<IStaffProfileCommandService>().Should().NotBeNull();

        // Query Services
        sp.GetService<IGuestProfileQueryService>().Should().NotBeNull();
        sp.GetService<IStaffProfileQueryService>().Should().NotBeNull();

        // ACL Facades
        sp.GetService<IGuestProfilesContextFacade>().Should().NotBeNull();
        sp.GetService<IStaffProfilesContextFacade>().Should().NotBeNull();
    }
}
