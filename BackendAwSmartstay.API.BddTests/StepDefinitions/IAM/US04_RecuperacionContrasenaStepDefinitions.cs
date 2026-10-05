using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.StepDefinitions.IAM;

[Binding]
[Scope(Feature = "Recuperación de contraseña")]
public class US04_RecuperacionContrasenaStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private AppDbContext Db => _scenarioContext.Get<AppDbContext>();
    private IamTestContext? _iam;
    private IamTestContext Iam => _iam ??= new IamTestContext(Db);

    private string _trackedEmail = string.Empty;
    private string _recoveryToken = string.Empty;
    private Exception? _caughtException;

    public US04_RecuperacionContrasenaStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"que existe un usuario registrado y verificado con correo ""(.*)""")]
    public async Task GivenQueExisteUnUsuarioRegistradoYVerificado(string email)
    {
        _trackedEmail = email;
        var command = new SignUpCommand("Huésped", "Recuperación", email, "ContrasenaInicial.2026!");
        var user = await Iam.AuthenticationCommandService.Handle(command);
        user.VerifyEmail(DateTimeOffset.UtcNow);
        await Iam.UnitOfWork.CompleteAsync();
    }

    [Given(@"que el usuario con correo ""(.*)"" tiene un token válido de recuperación de contraseña")]
    public async Task GivenQueElUsuarioTieneUnTokenValidoDeRecuperacion(string email)
    {
        _trackedEmail = email;
        var command = new SignUpCommand("Huésped", "Olvido", email, "ContrasenaInicial.2026!");
        var user = await Iam.AuthenticationCommandService.Handle(command);
        user.VerifyEmail(DateTimeOffset.UtcNow);
        await Iam.UnitOfWork.CompleteAsync();

        var issued = await Iam.AccountTokenIssuer.IssueAsync(user, AccountTokenPurpose.PasswordReset);
        await Iam.UnitOfWork.CompleteAsync();
        _recoveryToken = issued.Value;
    }

    [Given(@"que se tiene un token de recuperación inválido ""(.*)""")]
    public void GivenQueSeTieneUnTokenInvalido(string invalidToken)
    {
        _recoveryToken = invalidToken;
    }

    [When(@"el usuario solicita la recuperación de su contraseña para el correo ""(.*)""")]
    public async Task WhenElUsuarioSolicitaLaRecuperacionDeSuContrasena(string email)
    {
        try
        {
            var command = new RequestPasswordRecoveryCommand(email);
            await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"el usuario envía el token de recuperación con la nueva contraseña ""(.*)""")]
    public async Task WhenElUsuarioEnviaElTokenDeRecuperacion(string newPassword)
    {
        try
        {
            var command = new ResetPasswordCommand(_recoveryToken, newPassword);
            await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"se intenta restablecer la contraseña con el token inválido y la nueva contraseña ""(.*)""")]
    public async Task WhenSeIntentaRestablecerConTokenInvalido(string newPassword)
    {
        try
        {
            var command = new ResetPasswordCommand(_recoveryToken, newPassword);
            await Iam.AuthenticationCommandService.Handle(command);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"el sistema debe emitir un token seguro de recuperación y enviar el enlace al correo del usuario")]
    public void ThenElSistemaDebeEmitirUnTokenYEnviarEnlace()
    {
        _caughtException.Should().BeNull();
        Iam.Notifications.SentPasswordResetLinks.Should().Contain(_trackedEmail);
    }

    [Then(@"la contraseña del usuario debe actualizarse correctamente")]
    public void ThenLaContrasenaDelUsuarioDebeActualizarseCorrectamente()
    {
        _caughtException.Should().BeNull();
        Iam.Notifications.SentPasswordChangedAlerts.Should().Contain(_trackedEmail);
    }

    [Then(@"el usuario puede iniciar sesión exitosamente con la nueva contraseña ""(.*)""")]
    public async Task ThenElUsuarioPuedeIniciarSesionConLaNuevaContrasena(string newPassword)
    {
        var signInCommand = new SignInCommand(_trackedEmail, newPassword);
        var result = await Iam.AuthenticationCommandService.Handle(signInCommand);
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"la solicitud de restablecimiento debe ser rechazada por token inválido")]
    public void ThenLaSolicitudDebeSerRechazadaPorTokenInvalido()
    {
        _caughtException.Should().NotBeNull();
        _caughtException.Should().BeOfType<InvalidAccountTokenException>();
    }
}
