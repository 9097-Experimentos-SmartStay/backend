using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.UnitTests.Payments.Domain;

[TestFixture]
public class CardDetailsTests
{
    private static CardDetails Card(string number) => new(number, "Ana Pérez", 12, 2030, "123");

    [TestCase("4242424242424242", "Visa")]
    [TestCase("340000000000009", "Amex")]
    [TestCase("370000000000002", "Amex")]
    [TestCase("5105105105105100", "Mastercard")]
    [TestCase("5555555555554444", "Mastercard")]
    [TestCase("2221000000000009", "Mastercard")]
    [TestCase("2720999999999999", "Mastercard")]
    [TestCase("6011111111111117", "Card")]
    [TestCase("5000000000000000", "Card")]
    [TestCase("2220999999999999", "Card")]
    public void Brand_IsInferredFromNumberPrefix(string number, string expectedBrand)
    {
        Assert.That(Card(number).Brand, Is.EqualTo(expectedBrand));
    }

    [Test]
    public void LastFour_ReturnsTheFinalDigits()
    {
        Assert.That(Card("4242424242424242").LastFour, Is.EqualTo("4242"));
    }

    [Test]
    public void MaskedLabel_ShowsBrandAndLastFourOnly()
    {
        Assert.That(Card("4111111111111234").MaskedLabel, Is.EqualTo("Visa ****1234"));
    }

    [Test]
    public void ToString_NeverRevealsFullNumberNorCvv()
    {
        var text = Card("4111111111111234").ToString();

        Assert.Multiple(() =>
        {
            Assert.That(text, Is.EqualTo("Visa ****1234"));
            Assert.That(text, Does.Not.Contain("4111111111111234"));
            Assert.That(text, Does.Not.Contain("123 "));
        });
    }
}
