using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using System.Globalization;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Notifications;

/// <summary>
///     Account e-mails (neutral Spanish) with links to the web application:
///     <c>/verify-email?token=</c>, <c>/reset-password?token=</c>, <c>/forgot-password</c> and <c>/login</c>.
/// </summary>
public class AccountEmailNotificationService(
    IEmailSender emailSender,
    IOptions<ApplicationUrlsSettings> urls,
    TimeProvider timeProvider) : IAccountNotificationService
{
    public Task SendEmailVerificationAsync(User user, string verificationToken, DateTimeOffset expiresAt) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(user))
            .Paragraph("Tu cuenta de SmartStay se creó correctamente. Confirma tu correo electrónico para completar el registro.")
            .Action("Confirmar mi correo", urls.Value.WebLink("verify-email", ("token", verificationToken)))
            .Footnote($"El enlace vence en {Remaining(expiresAt)}. Si no creaste esta cuenta, ignora este mensaje.")
            .To(user.Email.Value, "Confirma tu correo en SmartStay"));

    public Task SendAccountLockedAsync(User user, DateTimeOffset lockedUntil) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(user))
            .Paragraph("Detectamos varios intentos fallidos de inicio de sesión en tu cuenta, así que la bloqueamos temporalmente para protegerla.")
            .Paragraph($"Podrás volver a intentarlo en {Remaining(lockedUntil)}. Si no fuiste tú, te recomendamos restablecer tu contraseña.")
            .Action("Restablecer mi contraseña", urls.Value.WebLink("forgot-password"))
            .To(user.Email.Value, "Bloqueamos temporalmente tu cuenta de SmartStay"));

    public Task SendPasswordResetLinkAsync(User user, string resetToken, DateTimeOffset expiresAt) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(user))
            .Paragraph("Recibimos una solicitud para restablecer la contraseña de tu cuenta de SmartStay.")
            .Action("Crear una nueva contraseña", urls.Value.WebLink("reset-password", ("token", resetToken)))
            .Footnote($"El enlace es de un solo uso y vence en {Remaining(expiresAt)}. Si no lo solicitaste, ignora este mensaje: tu contraseña no cambiará.")
            .To(user.Email.Value, "Restablece tu contraseña de SmartStay"));

    public Task SendPasswordChangedAsync(User user) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(user))
            .Paragraph("Tu contraseña se actualizó correctamente y cerramos las sesiones abiertas en otros dispositivos.")
            .Paragraph("Si no hiciste este cambio, restablece tu contraseña de inmediato y contacta al administrador.")
            .Action("Iniciar sesión", urls.Value.WebLink("login"))
            .To(user.Email.Value, "Tu contraseña de SmartStay se actualizó"));

    public Task SendPermissionsChangedAsync(User user) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(user))
            .Paragraph($"Un administrador actualizó tus permisos en SmartStay. Tu rol ahora es: {RoleLabel(user.Role.Value)}.")
            .Paragraph("Por seguridad cerramos tus sesiones abiertas. Inicia sesión nuevamente para seguir trabajando con tus nuevos permisos.")
            .Action("Iniciar sesión", urls.Value.WebLink("login"))
            .Footnote("Si crees que se trata de un error, contacta al administrador de tu hotel.")
            .To(user.Email.Value, "Tus permisos cambiaron, inicia sesión nuevamente"));

    private static string RoleLabel(string role) => role switch
    {
        UserRoles.Guest => "huésped",
        UserRoles.Reception => "recepción",
        UserRoles.Housekeeping => "limpieza",
        UserRoles.Maintenance => "mantenimiento",
        UserRoles.Admin => "administrador del hotel",
        UserRoles.ChainAdmin => "administrador de la cadena",
        _ => role
    };

    private static string Greeting(User user) =>
        string.IsNullOrWhiteSpace(user.FirstName) ? "Hola:" : $"Hola, {user.FirstName}:";

    private string Remaining(DateTimeOffset until)
    {
        var remaining = until - timeProvider.GetUtcNow();
        if (remaining.TotalHours >= 1)
        {
            var hours = (int)Math.Round(remaining.TotalHours);
            return hours == 1 ? "1 hora" : $"{hours.ToString(CultureInfo.InvariantCulture)} horas";
        }
        var minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
        return minutes == 1 ? "1 minuto" : $"{minutes.ToString(CultureInfo.InvariantCulture)} minutos";
    }
}
