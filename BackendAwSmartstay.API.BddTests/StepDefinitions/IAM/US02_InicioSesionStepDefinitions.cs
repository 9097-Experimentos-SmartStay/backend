using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.IAM;

[Binding]
[Scope(Feature = "Inicio de sesión seguro")]
public class US02_InicioSesionStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private AppDbContext Db => _scenarioContext.Get<AppDbContext>();
    private IamTestContext? _iam;
    private IamTestContext Iam => _iam ??= new IamTestContext(Db);

    private AuthenticationResult? _authResult;
    private Exception? _caughtException;
    private string _trackedEmail = string.Empty;

    public US02_InicioSesionStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"que existe un usuario registrado con correo ""(.*)"" y contraseña ""(.*)""")]
    public async Task GivenQueExisteUnUsuarioRegistrado(string email, string password)
    {
        _trackedEmail = email;
        var existing = await Iam.UserRepository.FindByEmailAsync(new Email(email));
        if (existing == null)
        {
            var command = new SignUpCommand("Usuario", "Prueba", email, password);
            var user = await Iam.AuthenticationCommandService.Handle(command);
            user.VerifyEmail(DateTimeOffset.UtcNow);
            await Iam.UnitOfWork.CompleteAsync();
        }
    }

    [When(@"el usuario solicita iniciar sesión con correo ""(.*)"" y contraseña ""(.*)""")]
    public async Task WhenElUsuarioSolicitaIniciarSesion(string email, string password)
    {
        try
        {
            var command = new SignInCommand(email, password);
            _authResult = await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el usuario falla el inicio de sesión (.*) veces consecutivas con una contraseña errónea")]
    public async Task WhenElUsuarioFallaElInicioDeSesionConsecutivas(int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                var command = new SignInCommand(_trackedEmail, "ClaveTotalmenteErronea.123!");
                await Iam.AuthenticationCommandService.Handle(command);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }
    }

    [Then(@"el inicio de sesión debe ser exitoso")]
    public void ThenElInicioDeSesionDebeSerExitoso()
    {
        _caughtException.Should().BeNull();
        _authResult.Should().NotBeNull();
        _authResult!.User.Should().NotBeNull();
    }

    [Then(@"debe recibir un token de acceso JWT válido")]
    public void ThenDebeRecibirUnTokenDeAccesoJwtValido()
    {
        _authResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        _authResult.AccessTokenExpiresAt.Should().NotBeNull();
        _authResult.AccessTokenExpiresAt!.Value.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Then(@"la autenticación debe fallar con error de credenciales inválidas")]
    public void ThenLaAutenticacionDebeFallarConErrorDeCredencialesInvalidas()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidCredentialsException>();
    }

    [Then(@"la cuenta del usuario debe quedar bloqueada temporalmente")]
    public async Task ThenLaCuentaDelUsuarioDebeQuedarBloqueadaTemporalmente()
    {
        var user = await Iam.UserRepository.FindByEmailAsync(new Email(_trackedEmail));
        user.Should().NotBeNull();
        user!.IsLockedOut(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Then(@"un intento posterior debe ser rechazado por cuenta bloqueada")]
    public async Task ThenUnIntentoPosteriorDebeSerRechazadoPorCuentaBloqueada()
    {
        Func<Task> act = async () =>
        {
            var command = new SignInCommand(_trackedEmail, "ClaveSegura.2026!");
            await Iam.AuthenticationCommandService.Handle(command);
        };

        await act.Should().ThrowAsync<AccountTemporarilyLockedException>();
    }
}
