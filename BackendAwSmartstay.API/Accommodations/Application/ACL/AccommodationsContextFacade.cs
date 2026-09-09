using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Application.ACL;

/// <summary>
/// Implementation of the Accommodations Anti-Corruption Layer (ACL) facade.
/// Uses internal Accommodations application query capabilities to answer cross-context inquiries.
/// </summary>
public class AccommodationsContextFacade(IHotelQueryService hotelQueryService) : IAccommodationsContextFacade
{
    public async Task<bool> HotelExistsAsync(int hotelId)
    {
        if (hotelId <= 0)
            return false;

        var query = new GetHotelByIdQuery(hotelId);
        var hotel = await hotelQueryService.Handle(query);
        return hotel != null;
    }
}
