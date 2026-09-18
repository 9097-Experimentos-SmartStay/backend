using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.Queries;
using BackendAwSmartstay.API.Audit.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Audit.Interfaces.REST.Transform;

public static class AuditEntryResourceFromEntityAssembler
{
    public static AuditEntryResource ToResourceFromEntity(AuditEntry entry) => new(
        entry.Id, entry.OccurredAt, entry.Action.ToString(), entry.Outcome.ToString(),
        entry.ActorUserId, entry.ActorEmail, entry.TargetUserId, entry.TargetEmail, entry.HotelId,
        entry.IpAddress, ToResource(entry.Details));

    private static AuditDetailsResource? ToResource(AuditDetails? details) => details is null
        ? null
        : new AuditDetailsResource(details.Reason, details.Method, details.Role, details.PreviousRole, details.NewRole,
            details.LockedUntil, details.RemainingRecoveryCodes, details.PreviousHotelId, details.NewHotelId,
            details.PreviousChainId, details.NewChainId);

    public static PagedResource<AuditEntryResource> ToResourceFromPage(PagedResult<AuditEntry> page) => new(
        page.Items.Select(ToResourceFromEntity).ToList(), page.Page, page.PageSize, page.TotalCount, page.TotalPages);
}
