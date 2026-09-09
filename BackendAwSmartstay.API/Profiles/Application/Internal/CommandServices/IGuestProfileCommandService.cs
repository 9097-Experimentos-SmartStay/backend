using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;

public interface IGuestProfileCommandService
{
    Task<GuestProfile?> Handle(CreateGuestProfileCommand command);
    Task<GuestProfile?> Handle(LinkGuestToUserCommand command);
    Task<GuestProfile?> Handle(SetGuestIdentificationCommand command);
    Task<GuestProfile?> Handle(CorrectGuestIdentificationCommand command);
    Task<GuestProfile?> Handle(UpdateGuestContactInformationCommand command);
    Task<GuestProfile?> Handle(DeactivateGuestProfileCommand command);
    Task<GuestProfile?> Handle(ActivateGuestProfileCommand command);
}
