using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;

public class GuestProfileCommandService(
    IGuestProfileRepository guestProfileRepository,
    IUnitOfWork unitOfWork,
    IDomainEventPublisher domainEventPublisher) : IGuestProfileCommandService
{
    public async Task<GuestProfile?> Handle(CreateGuestProfileCommand command)
    {
        var name = new PersonName(command.FirstName, command.LastName);
        var phone = new PhoneNumber(command.Phone);
        var email = command.Email != null ? new EmailAddress(command.Email) : null;
        var document = command.DocumentType.HasValue && command.DocumentNumber != null
            ? new IdentificationDocument(command.DocumentType.Value, command.DocumentNumber)
            : null;
        var address = command.Street != null && command.Number != null && command.City != null && command.PostalCode != null && command.Country != null
            ? new StreetAddress(command.Street, command.Number, command.City, command.PostalCode, command.Country)
            : null;

        var guest = new GuestProfile(
            GuestProfileId.New(),
            name,
            phone,
            email,
            document,
            address,
            command.UserId);

        await guestProfileRepository.AddAsync(guest);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(guest.DomainEvents);
        guest.ClearDomainEvents();

        return guest;
    }

    public async Task<GuestProfile?> Handle(LinkGuestToUserCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.LinkToUser(command.UserId, command.VerifiedEmail);

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(guest.DomainEvents);
        guest.ClearDomainEvents();

        return guest;
    }

    public async Task<GuestProfile?> Handle(SetGuestIdentificationCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.SetIdentification(command.Document);

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        return guest;
    }

    public async Task<GuestProfile?> Handle(CorrectGuestIdentificationCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.CorrectIdentification(command.NewDocument, command.Reason, command.StaffUserId);

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(guest.DomainEvents);
        guest.ClearDomainEvents();

        return guest;
    }

    public async Task<GuestProfile?> Handle(UpdateGuestContactInformationCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.UpdateContactInformation(command.Phone, command.Address);

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        return guest;
    }

    public async Task<GuestProfile?> Handle(DeactivateGuestProfileCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.Deactivate();

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        return guest;
    }

    public async Task<GuestProfile?> Handle(ActivateGuestProfileCommand command)
    {
        var guest = await guestProfileRepository.FindByIdAsync(command.GuestProfileId);
        if (guest is null) return null;

        guest.Activate();

        guestProfileRepository.Update(guest);
        await unitOfWork.CompleteAsync();

        return guest;
    }
}
