using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(Payment entity)
    {
        return new PaymentResource(
            entity.Id,
            entity.BookingId,
            entity.TransactionId,
            entity.Amount,
            entity.Status.ToString(),
            entity.CardNumberMasked,
            entity.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss")
        );
    }
}