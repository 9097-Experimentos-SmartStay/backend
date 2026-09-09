using System;
using System.Linq;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Application.ACL;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Accommodations.Application.ACL;

public class AccommodationsContextFacadeTests
{
    private class FakeHotelQueryService : IHotelQueryService
    {
        public Func<GetHotelByIdQuery, Task<Hotel?>>? GetByIdHandler { get; set; }
        public Func<GetAllHotelsQuery, Task<System.Collections.Generic.IEnumerable<Hotel>>>? GetAllHandler { get; set; }

        public Task<Hotel?> Handle(GetHotelByIdQuery query) =>
            GetByIdHandler != null ? GetByIdHandler(query) : Task.FromResult<Hotel?>(null);

        public Task<System.Collections.Generic.IEnumerable<Hotel>> Handle(GetAllHotelsQuery query) =>
            GetAllHandler != null ? GetAllHandler(query) : Task.FromResult<System.Collections.Generic.IEnumerable<Hotel>>(Enumerable.Empty<Hotel>());
    }

    [Fact]
    public async Task HotelExistsAsync_WhenHotelExists_ShouldReturnTrue()
    {
        // Arrange
        const int validHotelId = 101;
        var fakeQueryService = new FakeHotelQueryService
        {
            GetByIdHandler = q => Task.FromResult<Hotel?>(q.HotelId == validHotelId ? new Hotel() : null)
        };
        var facade = new AccommodationsContextFacade(fakeQueryService);

        // Act
        var result = await facade.HotelExistsAsync(validHotelId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HotelExistsAsync_WhenHotelDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        const int nonExistentHotelId = 999999;
        var fakeQueryService = new FakeHotelQueryService
        {
            GetByIdHandler = _ => Task.FromResult<Hotel?>(null)
        };
        var facade = new AccommodationsContextFacade(fakeQueryService);

        // Act
        var result = await facade.HotelExistsAsync(nonExistentHotelId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HotelExistsAsync_WhenHotelIdIsZeroOrNegative_ShouldReturnFalse()
    {
        // Arrange
        var fakeQueryService = new FakeHotelQueryService();
        var facade = new AccommodationsContextFacade(fakeQueryService);

        // Act
        var resultZero = await facade.HotelExistsAsync(0);
        var resultNegative = await facade.HotelExistsAsync(-5);

        // Assert
        resultZero.Should().BeFalse();
        resultNegative.Should().BeFalse();
    }

    [Fact]
    public void AddAccommodationsContextServices_ShouldRegisterIAccommodationsContextFacade()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        builder.AddAccommodationsContextServices();
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        var facade = serviceProvider.GetService<IAccommodationsContextFacade>();
        facade.Should().NotBeNull();
        facade.Should().BeOfType<AccommodationsContextFacade>();
    }

    [Fact]
    public void IAccommodationsContextFacade_Contract_ShouldNotExposeInternalDomainEntities()
    {
        // Arrange & Act
        var methods = typeof(IAccommodationsContextFacade).GetMethods();

        // Assert: Methods in IAccommodationsContextFacade must only return primitive/value types or DTOs
        foreach (var method in methods)
        {
            var returnType = method.ReturnType;
            returnType.FullName.Should().NotContain("Hotel", "Internal domain entities must not be leaked across ACL");
            returnType.FullName.Should().NotContain("DbContext", "EF Core context must not be leaked across ACL");
        }
    }
}
