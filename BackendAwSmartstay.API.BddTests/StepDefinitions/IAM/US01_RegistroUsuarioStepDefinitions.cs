using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using FluentAssertions;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.IAM;

[Binding]
[Scope(Feature = "Registro de usuario con validación")]
public class US01_RegistroUsuarioStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private AppDbContext Db => _scenarioContext.Get<AppDbContext>();
    private IamTestContext? _iam;
    private IamTestContext Iam => _iam ??= new IamTestContext(Db);

    private User? _registeredUser;
    private Exception? _caughtException;

    public US01_RegistroUsuarioStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"que el correo electrónico ""(.*)"" no se encuentra registrado en el sistema")]
    public async Task GivenQueElCorreoNoSeEncuentraRegistrado(string email)
    {
        var existing = await Iam.UserRepository.FindByEmailAsync(new Email(email));
        if (existing != null)
        {
            Db.Set<User>().Remove(existing);
            await Db.SaveChangesAsync();
        }
    }

    [Given(@"que ya existe un usuario registrado con el correo ""(.*)""")]
    public async Task GivenQueYaExisteUnUsuarioRegistradoConElCorreo(string email)
    {
        var command = new SignUpCommand("Existente", "Usuario", email, "SecurePass.2026!");
        await Iam.AuthenticationCommandService.Handle(command);
    }

    [When(@"el usuario solicita registrarse con nombre ""(.*)"", apellido ""(.*)"", correo ""(.*)"" y contraseña ""(.*)""")]
    public async Task WhenElUsuarioSolicitaRegistrarse(string firstName, string lastName, string email, string password)
    {
        try
        {
            var command = new SignUpCommand(firstName, lastName, email, password);
            _registeredUser = await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"otro usuario intenta registrarse con el correo ""(.*)"" y contraseña ""(.*)""")]
    public async Task WhenOtroUsuarioIntentaRegistrarseConElCorreo(string email, string password)
    {
        try
        {
            var command = new SignUpCommand("Otro", "Huésped", email, password);
            _registeredUser = await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"el usuario debe registrarse exitosamente en el sistema")]
    public void ThenElUsuarioDebeRegistrarseExitosamente()
    {
        _caughtException.Should().BeNull();
        _registeredUser.Should().NotBeNull();
        _registeredUser!.Id.Should().BeGreaterThan(0);
    }

    [Then(@"el rol asignado al usuario debe ser ""(.*)""")]
    public void ThenElRolAsignadoAlUsuarioDebeSer(string expectedRole)
    {
        _registeredUser.Should().NotBeNull();
        _registeredUser!.Role.Value.Should().BeEquivalentTo(expectedRole);
    }

    [Then(@"el estado inicial de verificación de correo debe ser falso")]
    public void ThenElEstadoInicialDeVerificacionDebeSerFalso()
    {
        _registeredUser.Should().NotBeNull();
        _registeredUser!.EmailVerified.Should().BeFalse();
    }

    [Then(@"la solicitud de registro debe ser rechazada indicando que el correo ya está registrado")]
    public void ThenLaSolicitudDebeSerRechazadaPorCorreoDuplicado()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<EmailAlreadyRegisteredException>();
    }

    [Then(@"la solicitud de registro debe ser rechazada por contraseña no permitida")]
    public void ThenLaSolicitudDebeSerRechazadaPorContrasenaNoPermitida()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidFieldException>();
    }
}
