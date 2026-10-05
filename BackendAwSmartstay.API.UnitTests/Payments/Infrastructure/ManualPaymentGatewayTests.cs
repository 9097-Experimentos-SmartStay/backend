using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Infrastructure.Gateways;

namespace BackendAwSmartstay.API.UnitTests.Payments.Infrastructure;

[TestFixture]
public class ManualPaymentGatewayTests
{
    private readonly ManualPaymentGateway _gateway = new();

    private static PaymentCharge Charge(PaymentMethod method, string? operationNumber) =>
        new(5, "BK-0005", 450m, method, operationNumber);

    [Test]
    public async Task ChargeAsync_WithOperationNumber_ApprovesWithMethodAndOperationAsReference()
    {
        var result = await _gateway.ChargeAsync(Charge(PaymentMethod.Yape, "OP-123"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Approved, Is.True);
            Assert.That(result.TransactionReference, Is.EqualTo("YAPE-OP-123"));
            Assert.That(result.FailureReason, Is.Null);
        });
    }

    [Test]
    public async Task ChargeAsync_Cash_GeneratesCashReferenceEvenIfAnOperationNumberIsGiven()
    {
        var result = await _gateway.ChargeAsync(Charge(PaymentMethod.Cash, "IGNORED"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Approved, Is.True);
            Assert.That(result.TransactionReference, Does.Match("^CASH-BK-0005-[0-9A-F]{8}$"));
        });
    }

    [Test]
    public async Task ChargeAsync_NonCashWithoutOperationNumber_FallsBackToCashStyleReference()
    {
        var result = await _gateway.ChargeAsync(Charge(PaymentMethod.Plin, " "));

        Assert.That(result.TransactionReference, Does.StartWith("CASH-BK-0005-"));
    }

    [Test]
    public async Task ChargeAsync_CashReferences_AreUniquePerCall()
    {
        var first = await _gateway.ChargeAsync(Charge(PaymentMethod.Cash, null));
        var second = await _gateway.ChargeAsync(Charge(PaymentMethod.Cash, null));

        Assert.That(first.TransactionReference, Is.Not.EqualTo(second.TransactionReference));
    }
}
