using System;
using System.Linq;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using FluentAssertions;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.IAM;

[Binding]
[Scope(Feature = "Gestión de perfiles y roles")]
public class US03_GestionPerfilesRolesStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private AppDbContext Db => _scenarioContext.Get<AppDbContext>();

    private StaffProfile? _staffProfile;
    private Exception? _caughtException;
    private int _targetHotelId = 1;

    public US03_GestionPerfilesRolesStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private class StubAccommodationsFacade(bool hotelExists) : IAccommodationsContextFacade
    {
        public Task<RoomOffer?> FetchRoomAsync(int id) => Task.FromResult<RoomOffer?>(null);
        public Task<RoomOffer?> LockRoomForBookingAsync(int id) => Task.FromResult<RoomOffer?>(null);
        public Task<HotelSummary?> FetchHotelAsync(int id) => Task.FromResult<HotelSummary?>(null);
        public Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int id) => Task.FromResult<HotelPaymentInstructions?>(null);
        public Task<bool> HotelExistsAsync(int id) => Task.FromResult(hotelExists && id == 1);
        public Task<bool> RoomExistsAsync(int id) => Task.FromResult(true);
        public Task<decimal?> FetchRoomPricePerNightAsync(int id) => Task.FromResult<decimal?>(null);
        public Task<int?> FetchHotelIdOfRoomAsync(int id) => Task.FromResult<int?>(null);
        public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) =>
            Task.FromResult<IReadOnlyDictionary<int, string>>(new System.Collections.Generic.Dictionary<int, string>());
        public Task OccupyRoomForCheckInAsync(int id, int? guestUserId, string? guestEmail) => Task.CompletedTask;
        public Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? id) => Task.FromResult<IReadOnlyList<RoomOffer>>(Array.Empty<RoomOffer>());
    }

    private StaffProfileCommandService CreateService(bool hotelExists = true)
    {
        var repo = new StaffProfileRepository(Db);
        var generator = new EmployeeCodeGenerator(Db);
        var uow = new UnitOfWork(Db, new FakeDomainEventDispatcher());
        var publisher = new FakeDomainEventPublisher();
        var facade = new StubAccommodationsFacade(hotelExists);

        return new StaffProfileCommandService(repo, generator, uow, publisher, facade);
    }

    [Given(@"que existe un usuario del sistema con identificador (.*)")]
    public void GivenQueExisteUnUsuarioDelSistemaConIdentificador(int userId)
    {
        // Reference ID set for testing
    }

    [Given(@"que existe un hotel registrado con identificador (.*)")]
    public void GivenQueExisteUnHotelRegistradoConIdentificador(int hotelId)
    {
        _targetHotelId = hotelId;
    }

    [Given(@"que no existe ningún hotel con identificador (.*)")]
    public void GivenQueNoExisteNingunHotelConIdentificador(int hotelId)
    {
        _targetHotelId = hotelId;
    }

    [Given(@"existe un perfil de colaborador registrado en el sistema")]
    public async Task GivenExisteUnPerfilDeColaboradorRegistradoEnElSistema()
    {
        var service = CreateService();
        var command = new CreateStaffProfileCommand(
            new UserId(100),
            new PersonName("Laura", "Mendoza"),
            new EmailAddress("laura.mendoza@smartstay.pe"),
            new JobPosition("Recepcionista"),
            HabitualShift.Morning);

        _staffProfile = await service.Handle(command);
    }

    [When(@"el administrador crea el perfil de personal con cargo ""(.*)"", turno ""(.*)"" y correo ""(.*)""")]
    public async Task WhenElAdministradorCreaElPerfilDePersonal(string position, string shift, string email)
    {
        try
        {
            var service = CreateService();
            var habitualShift = shift.Equals("Mañana", StringComparison.OrdinalIgnoreCase) ? HabitualShift.Morning : HabitualShift.Night;
            var command = new CreateStaffProfileCommand(
                new UserId(100),
                new PersonName("Carlos", "Gomez"),
                new EmailAddress(email),
                new JobPosition(position),
                habitualShift);

            _staffProfile = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el administrador asigna al colaborador al hotel (.*) con el rol ""(.*)""")]
    public async Task WhenElAdministradorAsignaAlColaboradorAlHotel(int hotelId, string role)
    {
        try
        {
            var service = CreateService(hotelExists: true);
            var staffRole = Enum.Parse<StaffRole>(role, ignoreCase: true);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new AddStaffAssignmentCommand(
                _staffProfile!.Id,
                ScopeLevel.Hotel,
                new TargetId(hotelId),
                staffRole,
                new DateRange(today),
                today);

            _staffProfile = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el administrador intenta asignar al colaborador al hotel (.*) con el rol ""(.*)""")]
    public async Task WhenElAdministradorIntentaAsignarAlColaboradorAlHotelInexistente(int hotelId, string role)
    {
        try
        {
            var service = CreateService(hotelExists: false);
            var staffRole = Enum.Parse<StaffRole>(role, ignoreCase: true);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new AddStaffAssignmentCommand(
                _staffProfile!.Id,
                ScopeLevel.Hotel,
                new TargetId(hotelId),
                staffRole,
                new DateRange(today),
                today);

            _staffProfile = await service.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"el perfil de personal debe registrarse correctamente con un código de empleado generado")]
    public void ThenElPerfilDePersonalDebeRegistrarseCorrectamente()
    {
        _caughtException.Should().BeNull();
        _staffProfile.Should().NotBeNull();
        _staffProfile!.Code.Should().NotBeNull();
        _staffProfile.Code.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"el perfil debe estar vinculado al usuario (.*)")]
    public void ThenElPerfilDebeEstarVinculadoAlUsuario(int userId)
    {
        _staffProfile!.UserId.Value.Should().Be(userId);
    }

    [Then(@"la asignación al hotel debe registrarse exitosamente en el perfil del colaborador")]
    public void ThenLaAsignacionAlHotelDebeRegistrarseExitosamente()
    {
        _caughtException.Should().BeNull();
        _staffProfile.Should().NotBeNull();
        _staffProfile!.Assignments.Should().NotBeEmpty();
        _staffProfile.Assignments.Any(a => a.TargetId.Value == _targetHotelId).Should().BeTrue();
    }

    [Then(@"la solicitud de asignación debe ser rechazada indicando que el hotel no existe")]
    public void ThenLaSolicitudDeAsignacionDebeSerRechazada()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<DomainValidationException>();
    }
}
