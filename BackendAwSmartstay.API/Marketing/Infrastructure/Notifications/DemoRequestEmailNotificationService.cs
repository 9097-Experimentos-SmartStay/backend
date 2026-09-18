using BackendAwSmartstay.API.Marketing.Application.Internal.Configuration;
using BackendAwSmartstay.API.Marketing.Application.OutboundServices;
using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Marketing.Infrastructure.Notifications;

/// <summary>Demo request e-mails (neutral Spanish). Links go to the demo section of the landing (<c>/#demo</c>).</summary>
public class DemoRequestEmailNotificationService(
    IEmailSender emailSender,
    IOptions<ApplicationUrlsSettings> urls,
    IOptions<SalesSettings> sales) : IDemoRequestNotificationService
{
    private string ScheduleLink => urls.Value.LandingLink("#demo");

    public Task SendConfirmationAsync(DemoRequest request) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting($"Hola, {request.FirstName}:")
            .Paragraph($"Recibimos tu solicitud de demo de SmartStay para {request.HotelName}. Gracias por tu interés.")
            .Paragraph("Nuestro equipo comercial se pondrá en contacto contigo muy pronto. Si todavía no lo hiciste, puedes elegir ahora el día y la hora de tu demo.")
            .Action("Agendar mi demo", ScheduleLink)
            .Footnote($"Número de solicitud: {request.Id}. Si no enviaste esta solicitud, ignora este mensaje.")
            .To(request.Email, "Recibimos tu solicitud de demo de SmartStay"));

    public Task NotifySalesTeamAsync(DemoRequest request) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Paragraph($"Nueva solicitud de demo #{request.Id} recibida el {request.ReceivedAt.UtcDateTime:yyyy-MM-dd HH:mm} (UTC).")
            .Paragraph($"Contacto: {request.FirstName} {request.LastName} · {request.JobTitle} · {request.Email}" +
                       (request.Phone is null ? string.Empty : $" · {request.Phone}"))
            .Paragraph($"Establecimiento: {request.HotelName} · {Label(request.AccommodationType)} · " +
                       $"{Label(request.RoomsRange)} habitaciones · visitó la sección para {Label(request.Profile)} · " +
                       $"nos conoció por {Label(request.ReferralSource)}")
            .Paragraph(request.Message is null ? "Sin mensaje." : $"Mensaje: {request.Message}")
            .Footnote("Si nadie responde, el visitante recibirá un recordatorio automático.")
            .To(sales.Value.NotificationEmail, $"Nueva solicitud de demo: {request.HotelName}"));

    private static string Label(AccommodationType value) => value switch
    {
        AccommodationType.Boutique => "hotel boutique",
        AccommodationType.Alternative => "alojamiento alternativo",
        _ => "cadena hotelera"
    };

    private static string Label(RoomsRange value) => value switch
    {
        RoomsRange.From1To10 => "1 a 10",
        RoomsRange.From11To30 => "11 a 30",
        RoomsRange.From31To60 => "31 a 60",
        _ => "más de 60"
    };

    private static string Label(VisitorProfile value) => value == VisitorProfile.Admin ? "administradores" : "huéspedes";

    private static string Label(ReferralSource value) => value switch
    {
        ReferralSource.Search => "un buscador",
        ReferralSource.Social => "redes sociales",
        ReferralSource.Referral => "una recomendación",
        ReferralSource.Event => "un evento",
        _ => "otro medio"
    };

    public Task SendFollowUpAsync(DemoRequest request) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting($"Hola, {request.FirstName}:")
            .Paragraph($"Hace unos días nos pediste una demo de SmartStay para {request.HotelName} y queremos asegurarnos de que no te quedes sin verla.")
            .Paragraph("Elige el horario que mejor te acomode y te mostraremos cómo SmartStay simplifica la gestión de tu establecimiento.")
            .Action("Agendar mi demo", ScheduleLink)
            .Footnote("Si ya coordinaste tu demo con nuestro equipo, ignora este mensaje.")
            .To(request.Email, "¿Aún quieres ver SmartStay en acción?"));
}
