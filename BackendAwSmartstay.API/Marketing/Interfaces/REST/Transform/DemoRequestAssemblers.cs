using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.Commands;
using BackendAwSmartstay.API.Marketing.Domain.Model.Queries;
using BackendAwSmartstay.API.Marketing.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Marketing.Interfaces.REST.Transform;

public static class DemoRequestAssemblers
{
    /// <summary>Only called with a validated resource (the codes are known).</summary>
    public static SubmitDemoRequestCommand ToCommand(CreateDemoRequestResource resource) => new(
        resource.FirstName!, resource.LastName!, resource.HotelName!, resource.JobTitle!, resource.Email!,
        DemoRequestCodes.AccommodationTypes[resource.AccommodationType!],
        DemoRequestCodes.RoomsRanges[resource.RoomsRange!],
        DemoRequestCodes.ReferralSources[resource.ReferralSource!],
        DemoRequestCodes.Profiles[resource.Profile!],
        resource.Phone, resource.Message);

    public static DemoRequestResource ToResource(DemoRequest request) => new(
        request.Id, request.FirstName, request.LastName, request.HotelName, request.JobTitle, request.Email, request.Phone,
        DemoRequestCodes.Of(request.AccommodationType), DemoRequestCodes.Of(request.RoomsRange),
        DemoRequestCodes.Of(request.ReferralSource), DemoRequestCodes.Of(request.Profile), request.Message,
        request.Status.ToString(), request.ReceivedAt, request.FollowedUpAt);

    public static DemoRequestPageResource ToResource(DemoRequestPage page) => new(
        page.Items.Select(ToResource).ToList(), page.Page, page.PageSize, page.TotalCount, page.TotalPages);
}
