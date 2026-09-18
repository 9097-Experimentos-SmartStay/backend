using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.CommandServices;

/// <summary>
/// Service implementation for handling hotel commands.
/// Orchestrates the flow between the repository and the domain logic.
/// </summary>
public class HotelCommandService(
    IHotelRepository hotelRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork)
    : IHotelCommandService
{
    /// <summary>
    /// Handles the creation of a new hotel.
    /// </summary>
    /// <param name="command">The command containing the hotel creation data.</param>
    /// <returns>The created hotel or null if creation failed.</returns>
    public async Task<Hotel?> Handle(CreateHotelCommand command)
    {
        var registrant = command.Registrant;
        var alreadyHostsAHotel = !registrant.ManagesChain
                                 && await hotelRepository.ExistsByHostIdAsync(registrant.UserId);
        var hostId = HotelRegistrationPolicy.ResolveHost(registrant, command.RequestedHostId, alreadyHostsAHotel);

        var hotel = new Hotel(hostId, command);
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await hotelRepository.AddAsync(hotel);
            await unitOfWork.CompleteAsync();

            // D2: the hotel a hotel administrator registers is the one they administer (IAM owns that scope).
            if (HotelRegistrationPolicy.AssignsHotelToRegistrant(registrant))
                await iamContextFacade.AssignHotelToAdministratorAsync(registrant.UserId, hotel.Id);
        });
        return hotel;
    }

    /// <summary>
    /// Handles the update of an existing hotel.
    /// </summary>
    /// <param name="command">The command containing the hotel update data.</param>
    /// <returns>The updated hotel or null if the hotel was not found.</returns>
    public async Task<Hotel?> Handle(UpdateHotelCommand command)
    {
        var hotel = await hotelRepository.FindByIdAsync(command.Id);
        if (hotel is null) return null;

        // Apply domain logic update
        hotel.UpdateInformation(
            command.Name, 
            command.Address, 
            command.City, 
            command.Country, 
            command.ImageUrl, 
            command.Description, 
            command.Type, 
            command.Amenities);

        hotelRepository.Update(hotel);
        await unitOfWork.CompleteAsync();
        return hotel;
    }

    /// <summary>
    /// Handles the deletion of a hotel.
    /// </summary>
    /// <param name="command">The command containing the hotel deletion data.</param>
    /// <returns>The deleted hotel or null if the hotel was not found.</returns>
    public async Task<Hotel?> Handle(DeleteHotelCommand command)
    {
        var hotel = await hotelRepository.FindByIdAsync(command.Id);
        if (hotel is null) return null;

        hotelRepository.Remove(hotel);
        await unitOfWork.CompleteAsync();
        return hotel;
    }
}