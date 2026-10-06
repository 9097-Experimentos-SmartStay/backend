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
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.Accommodations;

[Binding]
public class US06_GestionEstadosHabitacionStepDefinitions
{
    private readonly ScenarioContext _context;
    private AppDbContext Db => _context.Get<AppDbContext>();

    private Room? _currentRoom;
    private Exception? _caughtException;

    public US06_GestionEstadosHabitacionStepDefinitions(ScenarioContext context)
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

    [Given(@"que existe una habitación ""(.*)"" en estado ""(.*)""")]
    public async Task GivenQueExisteUnaHabitacionEnEstado(string number, string initialStatus)
    {
        var targetStatus = Enum.Parse<RoomStatus>(initialStatus, ignoreCase: true);

        if (await Db.Set<Hotel>().FindAsync(1) is null)
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
            typeof(Hotel).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hotel, 1);
            await Db.Set<Hotel>().AddAsync(hotel);
        }

        if (await Db.Set<RoomType>().FindAsync(1) is null)
        {
            var roomType = new RoomType("Estándar", "Habitación estándar para dos personas")
            {
                Id = 1
            };
            await Db.Set<RoomType>().AddAsync(roomType);
        }
        await Db.SaveChangesAsync();

        var room = new Room(new CreateRoomCommand(
            HotelId: 1,
            RoomTypeId: 1,
            Number: number,
            Price: 120m,
            Description: "Habitación de prueba",
            Amenities: []));

        if (targetStatus != RoomStatus.Available)
        {
            if (targetStatus == RoomStatus.Cleaning)
            {
                // Transition: Available -> Occupied -> Cleaning
                room.ChangeStatus(RoomStatus.Occupied, RoomStatusChangeOrigin.Staff, 1, "staff@smartstay.pe", DateTimeOffset.UtcNow);
                room.ChangeStatus(RoomStatus.Cleaning, RoomStatusChangeOrigin.Staff, 1, "staff@smartstay.pe", DateTimeOffset.UtcNow);
            }
            else
            {
                room.ChangeStatus(targetStatus, RoomStatusChangeOrigin.Staff, 1, "staff@smartstay.pe", DateTimeOffset.UtcNow);
            }
        }

        await Db.Set<Room>().AddAsync(room);
        await Db.SaveChangesAsync();
        _currentRoom = room;
    }

    [When(@"el personal solicita cambiar el estado de la habitación ""(.*)"" a ""(.*)""")]
    public async Task WhenElPersonalSolicitaCambiarElEstadoDeLaHabitacionA(string number, string newStatus)
    {
        var targetStatus = Enum.Parse<RoomStatus>(newStatus, ignoreCase: true);

        try
        {
            var service = CreateService();
            var command = new ChangeRoomStatusCommand(
                RoomId: _currentRoom!.Id,
                Status: targetStatus,
                ChangedByUserId: 1,
                ChangedByEmail: "staff@smartstay.pe");

            _currentRoom = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"el estado de la habitación ""(.*)"" debe actualizarse a ""(.*)""")]
    public void ThenElEstadoDeLaHabitacionDebeActualizarseA(string number, string expectedStatus)
    {
        _caughtException.Should().BeNull();
        _currentRoom.Should().NotBeNull();
        _currentRoom!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then(@"la solicitud de cambio de estado debe ser rechazada por transición no permitida")]
    public void ThenLaSolicitudDeCambioDeEstadoDebeSerRechazadaPorTransicionNoPermitida()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidRoomStatusTransitionException>();
    }
}
