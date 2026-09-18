using System.Globalization;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Notifications;

/// <summary>Check-in e-mails to the hotel staff (neutral Spanish).</summary>
public class CheckInEmailNotificationService(IEmailSender emailSender, IOptions<ApplicationUrlsSettings> urls)
    : ICheckInNotificationService
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-PE");

    public async Task SendGuestCheckedInAsync(IReadOnlyList<UserContact> recipients, Booking booking, HotelSummary? hotel)
    {
        foreach (var recipient in recipients)
            await emailSender.SendAsync(EmailLayout.Create()
                .Greeting(Greeting(recipient))
                .Paragraph($"{booking.GuestName} completó el check-in digital en {hotel?.Name ?? "el hotel"}: habitación {booking.RoomId}, reserva {booking.Code}.")
                .Paragraph($"La estadía termina el {booking.CheckOutDate.ToString("dddd d 'de' MMMM", Spanish)}. Coordina el servicio de la habitación durante la estadía.")
                .Action("Ver el mapa de habitaciones", urls.Value.WebLink("rooms"))
                .To(recipient.Email, $"Check-in completado: habitación {booking.RoomId}"));
    }

    public async Task SendCheckInAssistanceRequestedAsync(IReadOnlyList<UserContact> recipients, Booking booking,
        HotelSummary? hotel, string? message)
    {
        foreach (var recipient in recipients)
        {
            var email = EmailLayout.Create()
                .Greeting(Greeting(recipient))
                .Paragraph($"{booking.GuestName} necesita ayuda con el check-in de la reserva {booking.Code} en {hotel?.Name ?? "el hotel"} (habitación {booking.RoomId}).")
                .Paragraph($"Contacto: {booking.GuestEmail}{(booking.GuestPhone is { } phone ? $", {phone}" : string.Empty)}.");
            if (message is not null) email.Paragraph($"Mensaje del huésped: \"{message}\"");
            await emailSender.SendAsync(email
                .Action("Ver la reserva", urls.Value.WebLink("bookings"))
                .To(recipient.Email, $"Ayuda con el check-in: reserva {booking.Code}"));
        }
    }

    private static string Greeting(UserContact contact) =>
        contact.FullName is null ? "Hola:" : $"Hola, {contact.FullName.Split(' ')[0]}:";
}
