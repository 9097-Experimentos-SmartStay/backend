using BackendAwSmartstay.API.Audit.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Audit.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Audit.Application.OutboundServices;
using BackendAwSmartstay.API.Audit.Domain.Repositories;
using BackendAwSmartstay.API.Audit.Domain.Services;
using BackendAwSmartstay.API.Audit.Infrastructure.Http;
using BackendAwSmartstay.API.Audit.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Events;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

namespace BackendAwSmartstay.API.Audit.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>Audit bounded context: the access audit log fed by the IAM events.</summary>
    public static void AddAuditContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRequestOriginProvider, HttpRequestOriginProvider>();
        builder.Services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();
        builder.Services.AddScoped<IAuditQueryService, AuditQueryService>();

        // Subscriptions to the IAM published events.
        builder.Services.AddScoped<IamAccessAuditHandler>();
        builder.Services.AddScoped<IDomainEventHandler<UserSignedInEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<SignInFailedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserLockedOutEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserSignedOutEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserPasswordResetEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserPasswordChangedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserCreatedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserRoleChangedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserDeactivatedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserActivatedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<MfaEnabledEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<MfaVerifiedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<MfaVerificationFailedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<MfaRecoveryCodeUsedEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<MfaResetEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
        builder.Services.AddScoped<IDomainEventHandler<UserSignedOutEverywhereEvent>>(sp => sp.GetRequiredService<IamAccessAuditHandler>());
    }
}
