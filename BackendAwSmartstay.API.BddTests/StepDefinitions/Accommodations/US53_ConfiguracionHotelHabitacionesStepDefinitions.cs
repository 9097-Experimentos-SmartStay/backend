using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Accommodations.Application.Internal.Configuration;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.Accommodations;

[Binding]
public class US53_ConfiguracionHotelHabitacionesStepDefinitions
{
    private readonly ScenarioContext _context;
    private AppDbContext Db => _context.Get<AppDbContext>();

    private int _hotelId = 1;
    private int _roomTypeId = 1;
    private Room? _createdRoom;
    private Exception? _caughtException;

    public US53_ConfiguracionHotelHabitacionesStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    private class StubRoomReservationsFacade : IRoomReservationsFacade
    {
        public Task<IReadOnlyDictionary<int, int>> CountActiveBookingsAsync(IReadOnlyCollection<int> roomIds) =>
            Task.FromResult<IReadOnlyDictionary<int, int>>(new Dictionary<int, int>());
    }

    private RoomCommandService CreateService()
    {
        IRoomRepository roomRepo = new RoomRepository(Db);
        IRoomStatusChangeRepository statusRepo = new RoomStatusChangeRepository(Db);
        IRoomTypeRepository roomTypeRepo = new RoomTypeRepository(Db);
        IUnitOfWork uow = new UnitOfWork(Db, new FakeDomainEventDispatcher());
        var settings = Options.Create(new RoomOperationsSettings());

        return new RoomCommandService(
            roomRepo,
            statusRepo,
            roomTypeRepo,
            new StubRoomReservationsFacade(),
            uow,
            settings,
            TimeProvider.System);
    }

    [Given(@"que existe un hotel con identificador (.*) y un tipo de habitación con identificador (.*)")]
    public async Task GivenQueExisteUnHotelConIdentificadorYUnTipoDeHabitacionConIdentificador(int hotelId, int roomTypeId)
    {
        _hotelId = hotelId;
        _roomTypeId = roomTypeId;

        // Seed Hotel and RoomType in DbContext if not present
        if (await Db.Set<Hotel>().FindAsync(hotelId) is null)
        {
            var command = new CreateHotelCommand(
                new HotelRegistrant(1, true, null),
                null,
                "Hotel SmartStay Miraflores",
                "Av. Larco 123",
                "Lima",
                "Perú",
                "",
                "Hotel céntrico",
                "Hotel",
                [],
                new SessionContext(null));
            var hotel = new Hotel(1, command);
            typeof(Hotel).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hotel, hotelId);
            await Db.Set<Hotel>().AddAsync(hotel);
        }

        if (await Db.Set<RoomType>().FindAsync(roomTypeId) is null)
        {
            var roomType = new RoomType("Estándar", "Habitación estándar para dos personas")
            {
                Id = roomTypeId
            };
            await Db.Set<RoomType>().AddAsync(roomType);
        }

        await Db.SaveChangesAsync();
    }

    [Given(@"ya existe una habitación registrada con número ""(.*)"" en el hotel (.*)")]
    public async Task GivenYaExisteUnaHabitacionRegistradaConNumeroEnElHotel(string number, int hotelId)
    {
        var service = CreateService();
        var command = new CreateRoomCommand(
            HotelId: hotelId,
            RoomTypeId: _roomTypeId,
            Number: number,
            Price: 150m,
            Description: "Habitación previa",
            Amenities: ["WiFi", "TV"]);

        await service.Handle(command);
    }

    [When(@"el administrador registra una nueva habitación con número ""(.*)"", precio de (.*) soles y descripción ""(.*)""")]
    public async Task WhenElAdministradorRegistraUnaNuevaHabitacionConNumeroPrecioDeSolesYDescripcion(string number, decimal price, string description)
    {
        try
        {
            var service = CreateService();
            var command = new CreateRoomCommand(
                HotelId: _hotelId,
                RoomTypeId: _roomTypeId,
                Number: number,
                Price: price,
                Description: description,
                Amenities: ["WiFi", "TV", "Aire acondicionado"]);

            _createdRoom = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el administrador intenta registrar una habitación con número ""(.*)"" y precio de (.*) soles")]
    public async Task WhenElAdministradorIntentaRegistrarUnaHabitacionConNumeroYPrecioDeSoles(string number, decimal price)
    {
        try
        {
            var service = CreateService();
            var command = new CreateRoomCommand(
                HotelId: _hotelId,
                RoomTypeId: _roomTypeId,
                Number: number,
                Price: price,
                Description: "Habitación inválida",
                Amenities: []);

            _createdRoom = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el administrador intenta registrar otra habitación con el mismo número ""(.*)"" en el hotel (.*)")]
    public async Task WhenElAdministradorIntentaRegistrarOtraHabitacionConElMismoNumeroEnElHotel(string number, int hotelId)
    {
        try
        {
            var service = CreateService();
            var command = new CreateRoomCommand(
                HotelId: hotelId,
                RoomTypeId: _roomTypeId,
                Number: number,
                Price: 160m,
                Description: "Habitación duplicada",
                Amenities: []);

            _createdRoom = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"la habitación debe crearse exitosamente")]
    public void ThenLaHabitacionDebeCrearseExitosamente()
    {
        _caughtException.Should().BeNull();
        _createdRoom.Should().NotBeNull();
    }

    [Then(@"la habitación debe tener el número ""(.*)""")]
    public void ThenLaHabitacionDebeTenerElNumero(string number)
    {
        _createdRoom!.Number.Should().Be(number);
    }

    [Then(@"la habitación debe tener el precio de (.*) soles")]
    public void ThenLaHabitacionDebeTenerElPrecioDeSoles(decimal price)
    {
        _createdRoom!.Price.Should().Be(price);
    }

    [Then(@"el estado inicial de la habitación debe ser ""(.*)""")]
    public void ThenElEstadoInicialDeLaHabitacionDebeSer(string status)
    {
        _createdRoom!.Status.ToString().Should().Be(status);
    }

    [Then(@"la solicitud de registro debe ser rechazada indicando precio fuera de rango")]
    public void ThenLaSolicitudDeRegistroDebeSerRechazadaIndicandoPrecioFueraDeRango()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidFieldException>();
    }

    [Then(@"la solicitud de registro debe ser rechazada por número de habitación duplicado")]
    public void ThenLaSolicitudDeRegistroDebeSerRechazadaPorNumeroDeHabitacionDuplicado()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<DuplicateRoomNumberException>();
    }
}
