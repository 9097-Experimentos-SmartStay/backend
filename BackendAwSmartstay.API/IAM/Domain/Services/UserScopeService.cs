using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>
/// Implementation of user scope isolation based on hotel and chain affiliation.
/// </summary>
public class UserScopeService : IUserScopeService
{
    public bool CanAccessUser(User accessor, User target)
    {
        if (accessor == null || target == null) return false;

        // A user can always access themselves
        if (accessor.Id == target.Id) return true;

        var role = accessor.Role.Value.ToLowerInvariant();

        // ChainAdmin without a chain = global super-admin
        if (role == UserRoles.ChainAdmin)
        {
            if (accessor.ChainId == null) return true;
            return target.ChainId == accessor.ChainId;
        }

        // Admin and all staff variants can access users within the same hotel
        if (role == UserRoles.Admin ||
            role == UserRoles.Staff ||
            role == UserRoles.Reception ||
            role == UserRoles.Housekeeping ||
            role == UserRoles.Maintenance)
        {
            return target.HotelId == accessor.HotelId;
        }

        // Guest has no access to other users
        return false;
    }

    public bool CanAccessHotel(User accessor, int? hotelId)
    {
        if (accessor == null) return false;

        var role = accessor.Role.Value.ToLowerInvariant();

        // ChainAdmin can access any hotel (chain relationship cannot be validated
        // without a Hotel aggregate, so we assume broad access for MVP)
        if (role == UserRoles.ChainAdmin)
            return true;

        // All other roles are restricted to their assigned hotel
        return accessor.HotelId == hotelId;
    }
}