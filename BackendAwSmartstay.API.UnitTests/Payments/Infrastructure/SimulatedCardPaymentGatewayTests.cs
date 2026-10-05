using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Infrastructure.Gateways;

namespace BackendAwSmartstay.API.UnitTests.Payments.Infrastructure;

[TestFixture]
public class SimulatedCardPaymentGatewayTests
{
    private readonly SimulatedCardPaymentGateway _gateway = new();

    private static readonly PaymentCharge Charge = new(5, "BK-0005", 450m, PaymentMethod.OnlineCard, "Visa ****4242");

    private static CardDetails Card(string number) => new(number, "Ana Pérez", 12, 2030, "123");

    [Test]
    public async Task ChargeCardAsync_ValidCard_ApprovesWithSimulatedReference()
    {
        var result = await _gateway.ChargeCardAsync(Charge, Card("4242424242424242"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Approved, Is.True);
            Assert.That(result.TransactionReference, Does.Match("^SIM-VISA-4242-[0-9A-F]{10}$"));
        });
    }

    [TestCase("4000000000000002", "The card was declined.")]
    [TestCase("4000000000009995", "The card has insufficient funds.")]
    [TestCase("4000000000000127", "The card's security code is incorrect.")]
    public async Task ChargeCardAsync_TestDeclineNumbers_AreRejectedWithTheirReason(string number, string reason)
    {
        var result = await _gateway.ChargeCardAsync(Charge, Card(number));

        Assert.Multiple(() =>
        {
            Assert.That(result.Approved, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(reason));
            Assert.That(result.TransactionReference, Is.Empty);
        });
    }
}
