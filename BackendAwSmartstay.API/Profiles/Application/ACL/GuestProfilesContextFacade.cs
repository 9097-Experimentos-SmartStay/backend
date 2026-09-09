using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;

namespace BackendAwSmartstay.API.Profiles.Application.ACL;

public class GuestProfilesContextFacade(
    IGuestProfileCommandService guestProfileCommandService,
    IGuestProfileQueryService guestProfileQueryService) : IGuestProfilesContextFacade
{
    public async Task<Guid?> CreateGuestProfileAsync(string firstName, string lastName, string phone, string? email = null, int? userId = null)
    {
        var command = new CreateGuestProfileCommand(
            firstName,
            lastName,
            phone,
            Email: email,
            UserId: userId.HasValue ? new UserId(userId.Value) : null);

        var guest = await guestProfileCommandService.Handle(command);
        return guest?.Id.Value;
    }

    public async Task<Guid?> FetchGuestProfileIdByUserIdAsync(int userId)
    {
        var query = new GetGuestProfileByUserIdQuery(new UserId(userId));
        var guest = await guestProfileQueryService.Handle(query);
        return guest?.Id.Value;
    }

    public async Task<Guid?> FetchGuestProfileIdByEmailAsync(string email)
    {
        var query = new GetGuestProfileByEmailQuery(new EmailAddress(email));
        var guest = await guestProfileQueryService.Handle(query);
        return guest?.Id.Value;
    }

    public async Task<bool> LinkGuestProfileToUserAsync(Guid guestProfileId, int userId, string email)
    {
        var command = new LinkGuestToUserCommand(
            new GuestProfileId(guestProfileId),
            new UserId(userId),
            new EmailAddress(email));

        var guest = await guestProfileCommandService.Handle(command);
        return guest != null;
    }
}
