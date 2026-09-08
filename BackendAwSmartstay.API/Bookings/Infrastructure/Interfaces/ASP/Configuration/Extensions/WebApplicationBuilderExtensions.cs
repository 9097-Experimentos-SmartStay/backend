using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddBookingsContextServices(this WebApplicationBuilder builder)
    {
        // Bookings Bounded Context Injection Configuration

        // Repositories
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();

        // Command Services
        builder.Services.AddScoped<IBookingCommandService, BookingCommandService>();

        // Query Services
        builder.Services.AddScoped<IBookingQueryService, BookingQueryService>();
    }
}

