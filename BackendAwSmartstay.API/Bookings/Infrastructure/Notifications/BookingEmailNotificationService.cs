using System.Globalization;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Notifications;

/// <summary>Booking e-mails to the guest (neutral Spanish), with a link to their bookings in the web app.</summary>
public class BookingEmailNotificationService(
    IEmailSender emailSender,
    IOptions<ApplicationUrlsSettings> urls,
    IOptions<BookingPolicySettings> settings) : IBookingNotificationService
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-PE");

    public Task SendBookingPlacedAsync(Booking booking, BookingPlace place, PaymentInstructions instructions)
    {
        var email = EmailLayout.Create()
            .Greeting(Greeting(booking))
            .Paragraph($"Recibimos tu reserva {booking.Code} en {HotelName(place.Hotel)}: habitación {place.RoomNumber}, {Stay(booking)}.")
            .Paragraph($"Total a pagar: {Money(booking.TotalPrice)} ({booking.Nights} {(booking.Nights == 1 ? "noche" : "noches")} × {Money(booking.PricePerNight)}).")
            .Paragraph($"Tu reserva queda pendiente hasta que registremos tu pago. Paga antes del {Deadline(booking.PaymentDueAt!.Value)}: si no, la reserva se cancela automáticamente y la habitación se libera.");
        foreach (var method in Methods(instructions))
            email.Paragraph(method);
        email.Paragraph($"Indica el código {booking.Code} al pagar y envía el número de operación a recepción para que registren tu pago.");
        return emailSender.SendAsync(email
            .Action("Ver mis reservas", urls.Value.WebLink("bookings"))
            .Footnote("Si no hiciste esta reserva, ignora este mensaje: se cancelará sola si no se paga.")
            .To(booking.GuestEmail, $"Reserva {booking.Code} recibida: completa tu pago"));
    }

    public Task SendBookingConfirmedAsync(Booking booking, BookingPlace place) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(booking))
            .Paragraph($"Registramos tu pago de {Money(booking.TotalPrice)}. Tu reserva {booking.Code} en {HotelName(place.Hotel)} está confirmada.")
            .Paragraph($"Habitación {place.RoomNumber}, {Stay(booking)}.")
            .Paragraph($"El día de tu llegada, indica el código {booking.Code} en la recepción del hotel.")
            .Action("Ver mi reserva", urls.Value.WebLink("bookings"))
            .To(booking.GuestEmail, $"Reserva {booking.Code} confirmada"));

    public Task SendBookingCancelledAsync(Booking booking, BookingPlace place)
    {
        var why = booking.CancellationReason switch
        {
            CancellationReason.PaymentNotReceived => "se canceló porque no recibimos el pago antes del plazo indicado",
            CancellationReason.GuestRequest => "se canceló a tu pedido",
            _ => "fue cancelada por el hotel"
        };
        var email = EmailLayout.Create()
            .Greeting(Greeting(booking))
            .Paragraph($"Tu reserva {booking.Code} en {HotelName(place.Hotel)} ({Stay(booking)}) {why}. La habitación quedó liberada.");
        if (booking.ConfirmedAt is not null)
            email.Paragraph($"Como la reserva estaba pagada, el hotel te devolverá {Money(booking.TotalPrice)} por el mismo medio de pago. Si tienes dudas, contacta a recepción.");
        return emailSender.SendAsync(email
            .Action("Buscar otra habitación", urls.Value.WebLink("bookings"))
            .To(booking.GuestEmail, $"Reserva {booking.Code} cancelada"));
    }

    public Task SendBookingRescheduledAsync(Booking booking, BookingPlace place, DateTime previousCheckIn,
        DateTime previousCheckOut, string previousRoomNumber) =>
        emailSender.SendAsync(EmailLayout.Create()
            .Greeting(Greeting(booking))
            .Paragraph($"El hotel modificó tu reserva {booking.Code} en {HotelName(place.Hotel)}.")
            .Paragraph($"Antes: habitación {previousRoomNumber}, del {Date(previousCheckIn)} al {Date(previousCheckOut)}.")
            .Paragraph($"Ahora: habitación {place.RoomNumber}, {Stay(booking)}. Total: {Money(booking.TotalPrice)}.")
            .Paragraph("Si no pediste este cambio, contacta a recepción.")
            .Action("Ver mi reserva", urls.Value.WebLink("bookings"))
            .To(booking.GuestEmail, $"Reserva {booking.Code} modificada"));

    private static IEnumerable<string> Methods(PaymentInstructions instructions)
    {
        if (instructions.YapeNumber is { } yape) yield return $"Yape: {yape} (a nombre de {instructions.AccountHolder}).";
        if (instructions.PlinNumber is { } plin) yield return $"Plin: {plin} (a nombre de {instructions.AccountHolder}).";
        if (instructions.BankAccountNumber is { } account)
            yield return $"Transferencia bancaria{(instructions.BankName is { } bank ? $" ({bank})" : string.Empty)}: cuenta {account}" +
                         $"{(instructions.BankAccountCci is { } cci ? $", CCI {cci}" : string.Empty)}, a nombre de {instructions.AccountHolder}.";
        yield return "También puedes pagar en efectivo o con tarjeta en la recepción del hotel.";
    }

    private static string Greeting(Booking booking) => $"Hola, {booking.GuestName}:";

    private static string HotelName(HotelSummary? hotel) => hotel?.Name ?? "nuestro hotel";

    private static string Stay(Booking booking) => $"del {Date(booking.CheckInDate)} al {Date(booking.CheckOutDate)}";

    private static string Date(DateTime date) => date.ToString("dddd d 'de' MMMM 'de' yyyy", Spanish);

    private static string Money(decimal amount) => $"S/ {amount.ToString("0.00", CultureInfo.InvariantCulture)}";

    private string Deadline(DateTimeOffset due)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(settings.Value.TimeZone);
        return TimeZoneInfo.ConvertTime(due, zone).ToString("dddd d 'de' MMMM 'a las' HH:mm", Spanish) + " (hora del hotel)";
    }
}
