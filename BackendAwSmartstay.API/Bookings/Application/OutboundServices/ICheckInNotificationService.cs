using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.OutboundServices;

/// <summary>E-mails to the hotel staff about digital check-ins (US-08). Called after the commit.</summary>
public interface ICheckInNotificationService
{
    /// <summary>Scenario 4: the guest checked in; housekeeping is informed.</summary>
    Task SendGuestCheckedInAsync(IReadOnlyList<UserContact> recipients, Booking booking, HotelSummary? hotel);

    /// <summary>Scenario 3: the guest asked for help with the check-in.</summary>
    Task SendCheckInAssistanceRequestedAsync(IReadOnlyList<UserContact> recipients, Booking booking, HotelSummary? hotel, string? message);
}
