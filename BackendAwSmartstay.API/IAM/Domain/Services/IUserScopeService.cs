using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>
/// Domain service that determines whether a user can access data
/// belonging to a specific hotel or chain (data isolation / multi-tenancy).
/// </summary>
public interface IUserScopeService
{
    /// <summary>
    /// Returns true if the accessor can view or operate on the target user
    /// based on hotel or chain affiliation.
    /// </summary>
    bool CanAccessUser(User accessor, User target);

    /// <summary>
    /// Returns true if the accessor can access data for the given hotel id.
    /// </summary>
    bool CanAccessHotel(User accessor, int? hotelId);
}