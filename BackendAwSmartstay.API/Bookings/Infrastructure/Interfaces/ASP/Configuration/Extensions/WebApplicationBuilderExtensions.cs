using BackendAwSmartstay.API.Bookings.Application.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Bookings.Application.OutboundServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Bookings.Infrastructure.Notifications;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddBookingsContextServices(this WebApplicationBuilder builder)
    {
        // Bookings Bounded Context Injection Configuration

        builder.Services.AddOptions<BookingPolicySettings>()
            .Bind(builder.Configuration.GetSection(BookingPolicySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddScoped<HotelCalendar>();

        // Repositories
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();

        // Guest e-mails, sent by the handlers of the booking events
        builder.Services.AddScoped<IBookingNotificationService, BookingEmailNotificationService>();
        builder.Services.AddScoped<BookingGuestNotificationHandler>();
        builder.Services.AddScoped<IDomainEventHandler<BookingCreatedEvent>>(sp => sp.GetRequiredService<BookingGuestNotificationHandler>());
        builder.Services.AddScoped<IDomainEventHandler<BookingConfirmedEvent>>(sp => sp.GetRequiredService<BookingGuestNotificationHandler>());
        builder.Services.AddScoped<IDomainEventHandler<BookingCancelledEvent>>(sp => sp.GetRequiredService<BookingGuestNotificationHandler>());
        builder.Services.AddScoped<IDomainEventHandler<BookingRescheduledEvent>>(sp => sp.GetRequiredService<BookingGuestNotificationHandler>());

        // Domain Services
        builder.Services.AddScoped<RoomAvailabilityService>();

        // Command Services
        builder.Services.AddScoped<IBookingCommandService, BookingCommandService>();

        // Query Services
        builder.Services.AddScoped<IBookingQueryService, BookingQueryService>();
        builder.Services.AddScoped<IRoomAvailabilityQueryService, RoomAvailabilityQueryService>();

        // ACL Facade
        builder.Services.AddScoped<IBookingsContextFacade, BookingsContextFacade>();
    }
}

