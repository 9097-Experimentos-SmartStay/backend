using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.Commands;
using BackendAwSmartstay.API.Marketing.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Marketing.Domain.Services;

public interface IDemoRequestCommandService
{
    Task<DemoRequest> Handle(SubmitDemoRequestCommand command);

    /// <summary>Returns how many requests got their follow-up in this run.</summary>
    Task<int> Handle(SendDemoFollowUpsCommand command);
}

public interface IDemoRequestQueryService
{
    Task<DemoRequestPage> Handle(GetDemoRequestsQuery query);

    Task<DemoRequest?> Handle(GetDemoRequestByIdQuery query);
}
