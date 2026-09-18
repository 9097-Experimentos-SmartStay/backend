using System.Security.Claims;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

/// <summary>Translates the authenticated user into the Bookings notion of a requester.</summary>
public static class BookingRequesterFromPrincipalAssembler
{
    public static BookingRequester ToBookingRequester(this ClaimsPrincipal user) =>
        user.IsGuest()
            ? BookingRequester.Guest(user.GetUserId(), user.GetUsername())
            : BookingRequester.HotelStaff(user.GetUserId());
}
