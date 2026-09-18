using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;

public static class ProcessPaymentCommandFromResourceAssembler
{
    public static ProcessPaymentCommand ToCommandFromResource(ProcessPaymentResource resource, int? guestUserId = null)
    {
        // resource.Amount is deliberately ignored: the amount is computed server-side.
        return new ProcessPaymentCommand(
            resource.BookingId,
            resource.PaymentMethod,
            resource.CardNumber,
            resource.CardHolderName,
            resource.ExpirationDate,
            resource.Cvv,
            guestUserId
        );
    }
}