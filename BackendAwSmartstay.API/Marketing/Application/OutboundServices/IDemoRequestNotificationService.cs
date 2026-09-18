using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Marketing.Application.OutboundServices;

/// <summary>E-mails of the demo request flow. Called only after the unit of work has committed.</summary>
public interface IDemoRequestNotificationService
{
    /// <summary>Scenario 1: immediate confirmation to the visitor.</summary>
    Task SendConfirmationAsync(DemoRequest request);

    /// <summary>Tells the sales team a new request arrived.</summary>
    Task NotifySalesTeamAsync(DemoRequest request);

    /// <summary>Scenario 4: automatic reminder to a visitor who got no answer yet.</summary>
    Task SendFollowUpAsync(DemoRequest request);
}
