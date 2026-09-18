using System.Globalization;
using BackendAwSmartstay.API.Accommodations.Application.OutboundServices;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Accommodations.Infrastructure.Notifications;

/// <summary>Room e-mails to the hotel staff (neutral Spanish) with a link to the room map of the web app.</summary>
public class RoomEmailNotificationService(
    IEmailSender emailSender,
    IOptions<ApplicationUrlsSettings> urls,
    TimeProvider timeProvider) : IRoomNotificationService
{
    public async Task SendStatusChangedAsync(IReadOnlyList<UserContact> recipients, RoomNotice room, RoomStatus from,
        RoomStatus to, string? changedBy)
    {
        foreach (var recipient in recipients)
            await emailSender.SendAsync(EmailLayout.Create()
                .Greeting(Greeting(recipient))
                .Paragraph($"La habitación {room.RoomId} de {room.HotelName} cambió de {Label(from)} a {Label(to)}" +
                           (changedBy is null ? "." : $" (cambio hecho por {changedBy})."))
                .Paragraph(Instruction(to))
                .Action("Ver el mapa de habitaciones", urls.Value.WebLink("rooms"))
                .To(recipient.Email, $"Habitación {room.RoomId}: {Label(to)}"));
    }

    public async Task SendMaintenanceOverdueAsync(IReadOnlyList<UserContact> recipients, RoomNotice room, DateTimeOffset maintenanceSince)
    {
        var hours = (int)Math.Floor((timeProvider.GetUtcNow() - maintenanceSince).TotalHours);
        foreach (var recipient in recipients)
            await emailSender.SendAsync(EmailLayout.Create()
                .Greeting(Greeting(recipient))
                .Paragraph($"La habitación {room.RoomId} de {room.HotelName} lleva {hours.ToString(CultureInfo.InvariantCulture)} horas en mantenimiento y no se puede reservar.")
                .Paragraph("Revisa con el equipo de mantenimiento si la reparación avanza o si la habitación ya puede volver a limpieza o a disponible.")
                .Action("Ver el mapa de habitaciones", urls.Value.WebLink("rooms"))
                .To(recipient.Email, $"Alerta: habitación {room.RoomId} en mantenimiento desde hace {hours.ToString(CultureInfo.InvariantCulture)} horas"));
    }

    private static string Greeting(UserContact contact) =>
        contact.FullName is null ? "Hola:" : $"Hola, {contact.FullName.Split(' ')[0]}:";

    private static string Label(RoomStatus status) => status switch
    {
        RoomStatus.Available => "Disponible",
        RoomStatus.Occupied => "Ocupada",
        RoomStatus.Cleaning => "Limpieza",
        _ => "Mantenimiento"
    };

    private static string Instruction(RoomStatus status) => status switch
    {
        RoomStatus.Cleaning => "Por favor, prepárala para el siguiente huésped y márcala como disponible al terminar.",
        RoomStatus.Occupied => "La habitación tiene un huésped: coordina el servicio de limpieza de la estadía.",
        RoomStatus.Maintenance => "Revisa la incidencia; mientras esté en mantenimiento no se puede reservar.",
        _ => "La habitación ya se puede asignar y reservar."
    };
}
