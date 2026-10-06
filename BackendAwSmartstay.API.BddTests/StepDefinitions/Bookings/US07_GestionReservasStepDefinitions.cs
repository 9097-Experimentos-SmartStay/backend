using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.Bookings;

[Binding]
[Scope(Feature = "Gestión centralizada de reservas")]
public class US07_GestionReservasStepDefinitions
{
    private readonly ScenarioContext _context;
    private AppDbContext Db => _context.Get<AppDbContext>();

    private Booking? _currentBooking;
    private int _bookingId;
    private int _guestUserId = 20;
    private string _guestEmail = "guest20@smartstay.pe";
    private int _roomId = 101;
    private int _hotelId = 1;

    public US07_GestionReservasStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    private class StubAccommodationsFacade(int hotelId, int roomId) : IAccommodationsContextFacade
    {
        public Task<RoomOffer?> FetchRoomAsync(int id) =>
            Task.FromResult<RoomOffer?>(new RoomOffer(roomId, hotelId, 1, "Double", 120m, "Habitacion", [], "Available", "101", true));
        public Task<RoomOffer?> LockRoomForBookingAsync(int id) =>
            Task.FromResult<RoomOffer?>(new RoomOffer(roomId, hotelId, 1, "Double", 120m, "Habitacion", [], "Available", "101", true));
        public Task<HotelSummary?> FetchHotelAsync(int id) => Task.FromResult<HotelSummary?>(null);
        public Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int id) => Task.FromResult<HotelPaymentInstructions?>(null);
        public Task<bool> HotelExistsAsync(int id) => Task.FromResult(true);
        public Task<bool> RoomExistsAsync(int id) => Task.FromResult(true);
        public Task<decimal?> FetchRoomPricePerNightAsync(int id) => Task.FromResult<decimal?>(120m);
        public Task<int?> FetchHotelIdOfRoomAsync(int id) => Task.FromResult<int?>(hotelId);
        public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) =>
            Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string> { { roomId, "101" } });
        public Task OccupyRoomForCheckInAsync(int id, int? guestUserId, string? guestEmail) => Task.CompletedTask;
        public Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? id) => Task.FromResult<IReadOnlyList<RoomOffer>>(Array.Empty<RoomOffer>());
    }

    private class StubGuestProfilesFacade : IGuestProfilesContextFacade
    {
        public Task<Guid?> FetchGuestProfileIdByUserIdAsync(int userId) => Task.FromResult<Guid?>(null);
        public Task<Guid?> FetchGuestProfileIdByEmailAsync(string email) => Task.FromResult<Guid?>(null);
        public Task<Guid?> CreateGuestProfileAsync(string firstName, string lastName, string phone, string? email = null, int? userId = null) => Task.FromResult<Guid?>(null);
        public Task<bool> LinkGuestProfileToUserAsync(Guid guestProfileId, int userId, string email) => Task.FromResult(true);
    }

    private class StubIamFacade : IIamContextFacade
    {
        public Task<UserContact?> FetchUserContactAsync(int userId) => Task.FromResult<UserContact?>(new UserContact(userId, "guest20@smartstay.pe", "Guest 20", "Guest"));
        public Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles) => Task.FromResult<IReadOnlyList<UserContact>>(Array.Empty<UserContact>());
        public Task<int> FetchUserIdByEmail(string email) => Task.FromResult(20);
        public Task<string> FetchEmailByUserId(int userId) => Task.FromResult("guest20@smartstay.pe");
        public Task<ReissuedSession?> AssignHotelToAdministratorAsync(int userId, int hotelId, SessionContext currentSession) => Task.FromResult<ReissuedSession?>(null);
    }

    private BookingCommandService CreateService()
    {
        IBookingRepository bookingRepo = new BookingRepository(Db);
        IUnitOfWork uow = new UnitOfWork(Db, new FakeDomainEventDispatcher());
        var options = Options.Create(new BookingPolicySettings());
        var calendar = new HotelCalendar(options, TimeProvider.System);
        var availability = new RoomAvailabilityService(bookingRepo);
        var accommodations = new StubAccommodationsFacade(_hotelId, _roomId);

        return new BookingCommandService(
            bookingRepo,
            uow,
            new StubGuestProfilesFacade(),
            accommodations,
            new StubIamFacade(),
            availability,
            calendar,
            options,
            NullLogger<BookingCommandService>.Instance);
    }

    [Given(@"que existe una reserva en estado ""(.*)"" con identificador (.*)")]
    public async Task GivenQueExisteUnaReservaEnEstado(string status, int bookingId)
    {
        var service = CreateService();
        var command = new CreateBookingCommand(
            Requester: BookingRequester.Guest(_guestUserId, _guestEmail),
            RoomId: _roomId,
            GuestName: "Huesped SmartStay",
            GuestEmail: _guestEmail,
            CheckInDate: DateTime.UtcNow.Date.AddDays(10),
            CheckOutDate: DateTime.UtcNow.Date.AddDays(15),
            UserId: _guestUserId);

        _currentBooking = await service.Handle(command);
        _bookingId = _currentBooking.Id;
    }

    [Given(@"que existe una reserva confirmada con identificador (.*) del huésped con identificador (.*)")]
    public async Task GivenQueExisteUnaReservaConfirmada(int bookingId, int guestId)
    {
        _guestUserId = guestId;
        _guestEmail = $"guest{guestId}@smartstay.pe";
        var service = CreateService();
        var command = new CreateBookingCommand(
            Requester: BookingRequester.Guest(_guestUserId, _guestEmail),
            RoomId: _roomId,
            GuestName: "Huesped Confirmado",
            GuestEmail: _guestEmail,
            CheckInDate: DateTime.UtcNow.Date.AddDays(10),
            CheckOutDate: DateTime.UtcNow.Date.AddDays(15),
            UserId: _guestUserId);

        _currentBooking = await service.Handle(command);
        _bookingId = _currentBooking.Id;

        // Confirm
        _currentBooking = await service.Handle(new ConfirmBookingCommand(_bookingId));
    }

    [Given(@"que existe una reserva activa con identificador (.*) para la habitación (.*)")]
    public async Task GivenQueExisteUnaReservaActiva(int bookingId, int roomId)
    {
        _roomId = roomId;
        var service = CreateService();
        var command = new CreateBookingCommand(
            Requester: BookingRequester.Guest(_guestUserId, _guestEmail),
            RoomId: _roomId,
            GuestName: "Huesped Reprogramar",
            GuestEmail: _guestEmail,
            CheckInDate: DateTime.UtcNow.Date.AddDays(10),
            CheckOutDate: DateTime.UtcNow.Date.AddDays(15),
            UserId: _guestUserId);

        _currentBooking = await service.Handle(command);
        _bookingId = _currentBooking.Id;
    }

    [When(@"el sistema procesa la confirmación de la reserva con identificador (.*)")]
    public async Task WhenElSistemaProcesaLaConfirmacionDeLaReserva(int bookingId)
    {
        var service = CreateService();
        _currentBooking = await service.Handle(new ConfirmBookingCommand(_bookingId));
    }

    [When(@"el huésped solicita cancelar la reserva con identificador (.*)")]
    public async Task WhenElHuespedSolicitaCancelarLaReserva(int bookingId)
    {
        var service = CreateService();
        var requester = BookingRequester.Guest(_guestUserId, _guestEmail);
        _currentBooking = await service.Handle(new CancelBookingCommand(_bookingId, requester));
    }

    [When(@"el staff solicita reprogramar la reserva (.*) a las fechas ""(.*)"" hasta ""(.*)""")]
    public async Task WhenElStaffSolicitaReprogramarLaReserva(int bookingId, string checkIn, string checkOut)
    {
        var service = CreateService();
        var requester = BookingRequester.HotelStaff(userId: 1, hotelId: _hotelId);
        var command = new RescheduleBookingCommand(
            _bookingId,
            requester,
            DateTime.Parse(checkIn),
            DateTime.Parse(checkOut),
            RoomId: _roomId);

        _currentBooking = await service.Handle(command);
    }

    [Then(@"la reserva debe cambiar su estado a ""(.*)""")]
    public void ThenLaReservaDebeCambiarSuEstadoA(string expectedStatus)
    {
        _currentBooking.Should().NotBeNull();
        _currentBooking!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then(@"las fechas de estadía de la reserva deben actualizarse correctamente")]
    public void ThenLasFechasDeEstadiaDebenActualizarseCorrectamente()
    {
        _currentBooking.Should().NotBeNull();
        _currentBooking!.CheckInDate.Should().Be(new DateTime(2026, 12, 10));
        _currentBooking.CheckOutDate.Should().Be(new DateTime(2026, 12, 15));
    }
}
