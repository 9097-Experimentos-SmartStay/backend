using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>
///     US-03 scenario 1 + D2: which hotel an account created by an administrator belongs to.
///     <list type="bullet">
///         <item>A hotel administrator creates users only for the hotel they administer (it is the default when
///         no hotel is sent; any other hotel is forbidden).</item>
///         <item>A chain administrator chooses the hotel; front-desk and operations staff (reception, housekeeping,
///         maintenance) must belong to one.</item>
///     </list>
/// </summary>
public static class StaffAccountPolicy
{
    private static readonly HashSet<string> HotelBoundRoles =
        [UserRoles.Reception, UserRoles.Housekeeping, UserRoles.Maintenance];

    /// <summary>Returns the hotel of the new account.</summary>
    /// <exception cref="UnauthorizedOperationException">A hotel administrator asks for another hotel.</exception>
    /// <exception cref="BusinessRuleViolationException">A hotel administrator has no hotel yet.</exception>
    /// <exception cref="DomainValidationException">A staff role without a hotel.</exception>
    public static int? ResolveHotel(User actor, string role, int? requestedHotelId)
    {
        if (actor.Role.Value == UserRoles.Admin)
        {
            if (actor.HotelId is null)
                throw new BusinessRuleViolationException(IamErrorCodes.AdminWithoutHotel, "Register your hotel before creating staff users.");
            if (requestedHotelId is not null && requestedHotelId != actor.HotelId)
                throw new UnauthorizedOperationException(IamErrorCodes.HotelOutOfScope, "A hotel administrator can only create users for their own hotel.");
            return actor.HotelId;
        }

        if (requestedHotelId is null && HotelBoundRoles.Contains(role))
            throw new DomainValidationException(IamErrorCodes.HotelRequired, $"A '{role}' user must belong to a hotel: send hotelId.");
        return requestedHotelId;
    }
}
