using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.QueryServices;
/// <summary>
    /// Provides query handling services for retrieving hotel information, 
    /// including fetching a specific hotel by ID or listing all hotels.
    /// </summary>
public class HotelQueryService(IHotelRepository hotelRepository) : IHotelQueryService
{/// <summary>
    /// Retrieves a hotel based on the provided hotel identifier.
    /// </summary>
    /// <param name="query">Query containing the HotelId to search for.</param>
    /// <returns>The matching <see cref="Hotel"/> if found; otherwise null.</returns>
    public async Task<Hotel?> Handle(GetHotelByIdQuery query)
    {
        return await hotelRepository.FindByIdAsync(query.HotelId);
    }
    /// <summary>
    /// Retrieves a list of all hotels available in the system.
    /// </summary>
    /// <param name="query">Query used to trigger the operation.</param>
    /// <returns>An enumerable collection of <see cref="Hotel"/>.</returns>
    public async Task<IEnumerable<Hotel>> Handle(GetAllHotelsQuery query)
    {
        return await hotelRepository.ListAsync();
    }
}