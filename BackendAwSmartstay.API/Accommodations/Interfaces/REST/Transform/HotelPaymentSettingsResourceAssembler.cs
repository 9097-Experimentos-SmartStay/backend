using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class HotelPaymentSettingsResourceAssembler
{
    public static HotelPaymentSettingsResource ToResourceFromEntity(Hotel hotel)
    {
        var settings = hotel.PaymentSettings;
        return new HotelPaymentSettingsResource(hotel.Id, hotel.AcceptsBookings, settings?.AccountHolder,
            settings?.YapeNumber, settings?.PlinNumber, settings?.BankName, settings?.BankAccountNumber,
            settings?.BankAccountCci);
    }

    public static ConfigureHotelPaymentSettingsCommand ToCommandFromResource(int hotelId, UpdateHotelPaymentSettingsResource resource) =>
        new(hotelId, resource.AccountHolder, resource.YapeNumber, resource.PlinNumber, resource.BankName,
            resource.BankAccountNumber, resource.BankAccountCci);
}
