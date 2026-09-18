using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Payments.Application.ACL;
using BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Payments.Application.Internal.Configuration;
using BackendAwSmartstay.API.Payments.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Infrastructure.Gateways;
using BackendAwSmartstay.API.Payments.Interfaces.ACL;
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

        builder.Services.AddOptions<PaymentInstructionsSettings>()
            .Bind(builder.Configuration.GetSection(PaymentInstructionsSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Payment gateway port: payments received by the hotel (an online gateway adapter would replace it here)
        builder.Services.AddScoped<IPaymentGateway, ManualPaymentGateway>();

        // ACL facade and subscriptions (R2: a cancelled paid booking gets its payment refunded)
        builder.Services.AddScoped<IPaymentsContextFacade, PaymentsContextFacade>();
        builder.Services.AddScoped<IDomainEventHandler<BookingCancelledEvent>, RefundPaymentOnBookingCancelledHandler>();

        // Repositories
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Command Services
        builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();

        // Query Services
        builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();
    }
}

