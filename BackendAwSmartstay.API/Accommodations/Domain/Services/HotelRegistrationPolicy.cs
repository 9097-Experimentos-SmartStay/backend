using BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
///     Who may register a hotel and who hosts it (team decision D2, US-42):
///     <list type="bullet">
///         <item>a chain administrator registers any number of hotels and may register one on behalf of another host;</item>
///         <item>a hotel administrator registers only ONE hotel, their own: they become its host and it becomes the
///         hotel they administer. An administrator who already has a hotel cannot register another one.</item>
///     </list>
/// </summary>
public static class HotelRegistrationPolicy
{
    /// <summary>Returns the host of the new hotel or throws when the registrant may not register one.</summary>
    /// <param name="registrant">Who registers the hotel.</param>
    /// <param name="requestedHostId">Host requested in the request (only honoured for chain administrators).</param>
    /// <param name="registrantAlreadyHostsAHotel">True when a hotel hosted by the registrant already exists.</param>
    /// <exception cref="BusinessRuleViolationException">A hotel administrator already has a hotel.</exception>
    public static int ResolveHost(HotelRegistrant registrant, int? requestedHostId, bool registrantAlreadyHostsAHotel)
    {
        if (registrant.ManagesChain)
            return requestedHostId is > 0 ? requestedHostId.Value : registrant.UserId;

        if (registrant.AssignedHotelId is not null || registrantAlreadyHostsAHotel)
            throw new BusinessRuleViolationException(AccommodationErrorCodes.AdminAlreadyHasHotel,
                "A hotel administrator manages a single hotel and already has one. Ask a chain administrator to register more hotels.");

        return registrant.UserId;
    }

    /// <summary>True when registering the hotel also makes it the hotel the registrant administers.</summary>
    public static bool AssignsHotelToRegistrant(HotelRegistrant registrant) => !registrant.ManagesChain;
}
