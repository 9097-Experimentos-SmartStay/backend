using BackendAwSmartstay.API.Marketing.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Marketing.Application.Internal.Configuration;
using BackendAwSmartstay.API.Marketing.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Marketing.Application.OutboundServices;
using BackendAwSmartstay.API.Marketing.Domain.Repositories;
using BackendAwSmartstay.API.Marketing.Domain.Services;
using BackendAwSmartstay.API.Marketing.Infrastructure.Notifications;
using BackendAwSmartstay.API.Marketing.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.Marketing.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>Marketing bounded context: demo requests of the landing (US-27).</summary>
    public static void AddMarketingContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<SalesSettings>()
            .Bind(builder.Configuration.GetSection(SalesSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddOptions<DemoRequestSettings>()
            .Bind(builder.Configuration.GetSection(DemoRequestSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddScoped<IDemoRequestRepository, DemoRequestRepository>();
        builder.Services.AddScoped<IDemoRequestCommandService, DemoRequestCommandService>();
        builder.Services.AddScoped<IDemoRequestQueryService, DemoRequestQueryService>();
        builder.Services.AddScoped<IDemoRequestNotificationService, DemoRequestEmailNotificationService>();
    }
}
