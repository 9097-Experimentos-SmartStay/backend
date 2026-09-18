using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Payments.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Infrastructure.Gateways;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Payments.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Payments.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.Payments.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPaymentsContextServices(this WebApplicationBuilder builder)
    {
        // Payments Bounded Context Injection Configuration

        // How guests pay is configured per hotel (US-53, Accommodations: HotelPaymentSettings), not here.

        // Payment gateway port: payments received by the hotel (an online gateway adapter would replace it here)
        builder.Services.AddScoped<IPaymentGateway, ManualPaymentGateway>();

        // Subscriptions (R2: a cancelled paid booking gets its payment refunded)
        builder.Services.AddScoped<IDomainEventHandler<BookingCancelledEvent>, RefundPaymentOnBookingCancelledHandler>();

        // Repositories
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Command Services
        builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();

        // Query Services
        builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();
    }
}

