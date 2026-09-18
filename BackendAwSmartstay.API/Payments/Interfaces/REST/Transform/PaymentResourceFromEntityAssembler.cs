using System.Globalization;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(Payment entity) => new(
        entity.Id,
        entity.BookingId,
        entity.TransactionId,
        entity.Amount,
        entity.Status.ToString(),
        entity.Method.ToString(),
        entity.OperationNumber,
        entity.Note,
        entity.RecordedByUserId,
        entity.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        entity.RefundedAt);
}
