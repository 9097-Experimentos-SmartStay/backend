using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.Queries;
using BackendAwSmartstay.API.Marketing.Domain.Repositories;
using BackendAwSmartstay.API.Marketing.Domain.Services;

namespace BackendAwSmartstay.API.Marketing.Application.Internal.QueryServices;

public class DemoRequestQueryService(IDemoRequestRepository demoRequestRepository) : IDemoRequestQueryService
{
    public Task<DemoRequestPage> Handle(GetDemoRequestsQuery query) => demoRequestRepository.SearchAsync(query);

    public Task<DemoRequest?> Handle(GetDemoRequestByIdQuery query) => demoRequestRepository.FindByIdAsync(query.Id);
}
