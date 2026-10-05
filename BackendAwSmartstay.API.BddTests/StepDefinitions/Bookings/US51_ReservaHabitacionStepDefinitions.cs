using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.Bookings;

[Binding]
public class US51_ReservaHabitacionStepDefinitions
{
    private readonly ScenarioContext _context;
    private AppDbContext Db => _context.Get<AppDbContext>();

    private int _hotelId = 1;
    private int _roomId = 101;
    private string _roomNumber = "101";
    private string _roomType = "Double";
    private decimal _roomPrice = 150m;
    private int _guestUserId = 10;
    private string _guestEmail = "juan.perez@smartstay.pe";
    private string _guestName = "Juan Perez";

    private Booking? _createdBooking;
    private Exception? _caughtException;

    public US51_ReservaHabitacionStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    private class StubAccommodationsFacade(int hotelId, int roomId, string roomNumber, decimal price) : IAccommodationsContextFacade
    {
        public Task<RoomOffer?> FetchRoomAsync(int id) =>
            Task.FromResult<RoomOffer?>(new RoomOffer(roomId, hotelId, 1, "Double", price, "Habitacion", [], "Available", roomNumber, true));
        public Task<RoomOffer?> LockRoomForBookingAsync(int id) =>
            Task.FromResult<RoomOffer?>(new RoomOffer(roomId, hotelId, 1, "Double", price, "Habitacion", [], "Available", roomNumber, true));
        public Task<HotelSummary?> FetchHotelAsync(int id) => Task.FromResult<HotelSummary?>(null);
        public Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int id) => Task.FromResult<HotelPaymentInstructions?>(null);
        public Task<bool> HotelExistsAsync(int id) => Task.FromResult(true);
        public Task<bool> RoomExistsAsync(int id) => Task.FromResult(true);
        public Task<decimal?> FetchRoomPricePerNightAsync(int id) => Task.FromResult<decimal?>(price);
        public Task<int?> FetchHotelIdOfRoomAsync(int id) => Task.FromResult<int?>(hotelId);
        public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) =>
            Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string> { { roomId, roomNumber } });
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
        public Task<UserContact?> FetchUserContactAsync(int userId) => Task.FromResult<UserContact?>(new UserContact(userId, "juan.perez@smartstay.pe", "Juan Perez", "Guest"));
        public Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles) => Task.FromResult<IReadOnlyList<UserContact>>(Array.Empty<UserContact>());
        public Task<int> FetchUserIdByEmail(string email) => Task.FromResult(1);
        public Task<string> FetchEmailByUserId(int userId) => Task.FromResult("juan.perez@smartstay.pe");
        public Task<ReissuedSession?> AssignHotelToAdministratorAsync(int userId, int hotelId, SessionContext currentSession) => Task.FromResult<ReissuedSession?>(null);
    }

    private BookingCommandService CreateService()
    {
        IBookingRepository bookingRepo = new BookingRepository(Db);
        IUnitOfWork uow = new UnitOfWork(Db, new FakeDomainEventDispatcher());
        var options = Options.Create(new BookingPolicySettings());
        var calendar = new HotelCalendar(options, TimeProvider.System);
        var availability = new RoomAvailabilityService(bookingRepo);
        var accommodations = new StubAccommodationsFacade(_hotelId, _roomId, _roomNumber, _roomPrice);

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

    [Given(@"que existe un hotel registrado y operativo")]
    public void GivenQueExisteUnHotelRegistradoYOperativo()
    {
        _hotelId = 1;
    }

    [Given(@"existe una habitación ""(.*)"" de tipo ""(.*)"" disponible con precio por noche de (.*) soles")]
    public void GivenExisteUnaHabitacionDeTipoDisponibleConPrecioPorNocheDeSoles(string number, string type, decimal price)
    {
        _roomNumber = number;
        _roomId = int.Parse(number);
        _roomType = type;
        _roomPrice = price;
    }

    [Given(@"existe un huésped registrado con correo ""(.*)"" y nombre ""(.*)""")]
    public void GivenExisteUnHuespedRegistradoConCorreoYNombre(string email, string name)
    {
        _guestEmail = email;
        _guestName = name;
        _guestUserId = 10;
    }

    [Given(@"existe una reserva activa para la habitación ""(.*)"" del ""(.*)"" al ""(.*)""")]
    public async Task GivenExisteUnaReservaActivaParaLaHabitacionDelAl(string number, string checkIn, string checkOut)
    {
        _roomNumber = number;
        _roomId = int.Parse(number);
        var service = CreateService();
        var command = new CreateBookingCommand(
            Requester: BookingRequester.Guest(20, "existing@smartstay.pe"),
            RoomId: _roomId,
            GuestName: "Existing Guest",
            GuestEmail: "existing@smartstay.pe",
            CheckInDate: DateTime.Parse(checkIn),
            CheckOutDate: DateTime.Parse(checkOut),
            UserId: 20);

        await service.Handle(command);
    }

    [When(@"el huésped solicita registrar una reserva desde ""(.*)"" hasta ""(.*)""")]
    public async Task WhenElHuespedSolicitaRegistrarUnaReservaDesdeHasta(string checkIn, string checkOut)
    {
        try
        {
            var service = CreateService();
            var command = new CreateBookingCommand(
                Requester: BookingRequester.Guest(_guestUserId, _guestEmail),
                RoomId: _roomId,
                GuestName: _guestName,
                GuestEmail: _guestEmail,
                CheckInDate: DateTime.Parse(checkIn),
                CheckOutDate: DateTime.Parse(checkOut),
                UserId: _guestUserId);

            _createdBooking = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"otro huésped intenta registrar una reserva para la habitación ""(.*)"" del ""(.*)"" al ""(.*)""")]
    public async Task WhenOtroHuespedIntentaRegistrarUnaReservaParaLaHabitacionDelAl(string number, string checkIn, string checkOut)
    {
        _roomId = int.Parse(number);
        _roomNumber = number;
        try
        {
            var service = CreateService();
            var command = new CreateBookingCommand(
                Requester: BookingRequester.Guest(30, "other@smartstay.pe"),
                RoomId: _roomId,
                GuestName: "Other Guest",
                GuestEmail: "other@smartstay.pe",
                CheckInDate: DateTime.Parse(checkIn),
                CheckOutDate: DateTime.Parse(checkOut),
                UserId: 30);

            _createdBooking = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"un nuevo huésped solicita registrar una reserva para la habitación ""(.*)"" del ""(.*)"" al ""(.*)""")]
    public async Task WhenUnNuevoHuespedSolicitaRegistrarUnaReservaParaLaHabitacionDelAl(string number, string checkIn, string checkOut)
    {
        _roomId = int.Parse(number);
        _roomNumber = number;
        try
        {
            var service = CreateService();
            var command = new CreateBookingCommand(
                Requester: BookingRequester.Guest(40, "consecutive@smartstay.pe"),
                RoomId: _roomId,
                GuestName: "Consecutive Guest",
                GuestEmail: "consecutive@smartstay.pe",
                CheckInDate: DateTime.Parse(checkIn),
                CheckOutDate: DateTime.Parse(checkOut),
                UserId: 40);

            _createdBooking = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"la reserva debe crearse exitosamente")]
    public void ThenLaReservaDebeCrearseExitosamente()
    {
        _caughtException.Should().BeNull();
        _createdBooking.Should().NotBeNull();
    }

    [Then(@"el estado inicial de la reserva debe ser ""(.*)""")]
    public void ThenElEstadoInicialDeLaReservaDebeSer(string status)
    {
        _createdBooking!.Status.ToString().Should().Be(status);
    }

    [Then(@"la reserva debe contener un código de reserva generado")]
    public void ThenLaReservaDebeContenerUnCodigoDeReservaGenerado()
    {
        _createdBooking!.Code.Value.Should().NotBeNullOrWhiteSpace();
        _createdBooking.Code.Value.Should().StartWith("SS-");
    }

    [Then(@"la reserva debe estar asociada a la habitación ""(.*)""")]
    public void ThenLaReservaDebeEstarAsociadaALaHabitacion(string number)
    {
        _createdBooking!.RoomId.Should().Be(int.Parse(number));
    }

    [Then(@"la solicitud de reserva debe ser rechazada indicando que la habitación no está disponible")]
    public void ThenLaSolicitudDeReservaDebeSerRechazadaIndicandoQueLaHabitacionNoEstaDisponible()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<RoomNotAvailableException>();
    }

    [Then(@"la solicitud de reserva debe ser rechazada por rango de fechas inválido")]
    public void ThenLaSolicitudDeReservaDebeSerRechazadaPorRangoDeFechasInvalido()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<DomainValidationException>();
    }
}
