using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
///     Decides who may modify a hotel and its rooms.
/// </summary>
/// <remarks>
///     - chain_admin: any hotel (chains are not modelled on Hotel yet).<br/>
///     - admin: only the hotel assigned to them (User.HotelId) or hotels they host (Hotel.HostId).<br/>
///     - everyone else: read-only.
/// </remarks>
public static class HotelAccessPolicy
{
    public static bool CanManage(string role, int userId, int? userHotelId, Hotel hotel)
    {
        if (string.Equals(role, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            return (userHotelId.HasValue && userHotelId.Value == hotel.Id) || hotel.HostId == userId;

        return false;
    }
}
