using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;

public static class ProcessPaymentCommandFromResourceAssembler
{
    public static ProcessPaymentCommand ToCommandFromResource(ProcessPaymentResource resource)
    {
        return new ProcessPaymentCommand(
            resource.BookingId,
            resource.Amount,
            resource.PaymentMethod,
            resource.CardNumber,
            resource.CardHolderName,
            resource.ExpirationDate,
            resource.Cvv
        );
    }
}