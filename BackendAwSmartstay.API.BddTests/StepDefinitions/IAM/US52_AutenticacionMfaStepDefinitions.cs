using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.IAM;

[Binding]
[Scope(Feature = "Autenticación de dos factores para staff")]
public class US52_AutenticacionMfaStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private AppDbContext Db => _scenarioContext.Get<AppDbContext>();
    private IamTestContext? _iam;
    private IamTestContext Iam => _iam ??= new IamTestContext(Db);

    private User? _currentUser;
    private MfaEnrollment? _enrollment;
    private AuthenticationResult? _authResult;
    private TotpSecret? _mfaSecret;
    private Exception? _caughtException;

    public US52_AutenticacionMfaStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"que existe un usuario del staff con correo ""(.*)"" y contraseña ""(.*)""")]
    public async Task GivenQueExisteUnUsuarioDelStaff(string email, string password)
    {
        var command = new SignUpCommand("Staff", "Usuario", email, password);
        _currentUser = await Iam.AuthenticationCommandService.Handle(command);
        _currentUser.VerifyEmail(DateTimeOffset.UtcNow);
        await Iam.UnitOfWork.CompleteAsync();
    }

    [Given(@"que el usuario del staff ""(.*)"" tiene MFA habilitado con su clave secreta")]
    public async Task GivenQueElUsuarioDelStaffTieneMfaHabilitado(string email)
    {
        var command = new SignUpCommand("Staff", "Activo", email, "ClaveStaffSegura.2026!");
        _currentUser = await Iam.AuthenticationCommandService.Handle(command);
        _currentUser.VerifyEmail(DateTimeOffset.UtcNow);
        await Iam.UnitOfWork.CompleteAsync();

        var enrollCmd = new StartMfaEnrollmentCommand(_currentUser.Id);
        _enrollment = await Iam.MfaCommandService.Handle(enrollCmd);
        _mfaSecret = TotpSecret.FromBase32(_enrollment.Secret);

        var now = DateTimeOffset.UtcNow;
        var code = TotpAlgorithm.CodeAt(_mfaSecret, TotpAlgorithm.TimeStepAt(now) - 1);
        var confirmCmd = new ConfirmMfaEnrollmentCommand(_currentUser.Id, code, RememberMe: false);
        _authResult = await Iam.MfaCommandService.Handle(confirmCmd);
    }

    [When(@"el usuario solicita iniciar el enrolamiento en autenticación de dos factores")]
    public async Task WhenElUsuarioSolicitaIniciarElEnrolamiento()
    {
        try
        {
            var command = new StartMfaEnrollmentCommand(_currentUser!.Id);
            _enrollment = await Iam.MfaCommandService.Handle(command);
            _mfaSecret = TotpSecret.FromBase32(_enrollment.Secret);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"confirma el enrolamiento enviando el código TOTP generado por su aplicación")]
    public async Task WhenConfirmaElEnrolamientoEnviandoCodigoTotp()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var code = TotpAlgorithm.CodeAt(_mfaSecret!, TotpAlgorithm.TimeStepAt(now));
            var command = new ConfirmMfaEnrollmentCommand(_currentUser!.Id, code, RememberMe: false);
            _authResult = await Iam.MfaCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el usuario completa el primer factor de autenticación con su contraseña ""(.*)""")]
    public async Task WhenElUsuarioCompletaElPrimerFactor(string password)
    {
        try
        {
            var command = new SignInCommand(_currentUser!.Email.Value, password);
            _authResult = await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"envía el código TOTP correcto para el segundo factor")]
    public async Task WhenEnviaElCodigoTotpCorrecto()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var code = TotpAlgorithm.CodeAt(_mfaSecret!, TotpAlgorithm.TimeStepAt(now));
            var command = new VerifyMfaCommand(_currentUser!.Id, Code: code, RecoveryCode: null, RememberMe: false);
            _authResult = await Iam.MfaCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el usuario intenta verificar el segundo factor con un código inválido ""(.*)""")]
    public async Task WhenElUsuarioIntentaVerificarSegundoFactorConCodigoInvalido(string invalidCode)
    {
        try
        {
            var command = new VerifyMfaCommand(_currentUser!.Id, Code: invalidCode, RecoveryCode: null, RememberMe: false);
            _authResult = await Iam.MfaCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"la autenticación de dos factores debe quedar habilitada para el usuario")]
    public async Task ThenLaAutenticacionDeDosFactoresDebeQuedarHabilitada()
    {
        _caughtException.Should().BeNull();
        var user = await Iam.UserRepository.FindByIdAsync(_currentUser!.Id);
        user.Should().NotBeNull();
        user!.MfaEnabled.Should().BeTrue();
    }

    [Then(@"se deben generar códigos de recuperación de un solo uso")]
    public void ThenSeDebenGenerarCodigosDeRecuperacion()
    {
        _authResult.Should().NotBeNull();
        _authResult!.RecoveryCodes.Should().NotBeNull();
        _authResult.RecoveryCodes!.Count.Should().Be(10);
    }

    [Then(@"la sesión debe ser otorgada con un token de acceso JWT válido")]
    public void ThenLaSesionDebeSerOtorgadaConTokenValido()
    {
        _caughtException.Should().BeNull();
        _authResult.Should().NotBeNull();
        _authResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"la verificación de dos factores debe ser rechazada por código inválido")]
    public void ThenLaVerificacionDebeSerRechazadaPorCodigoInvalido()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidMfaCodeException>();
    }
}
