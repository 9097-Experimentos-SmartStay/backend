using BackendAwSmartstay.API.Payments.Application.Internal.Configuration;
using BackendAwSmartstay.API.Payments.Interfaces.ACL;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Payments.Application.ACL;

public class PaymentsContextFacade(IOptions<PaymentInstructionsSettings> instructions) : IPaymentsContextFacade
{
    public PaymentInstructions GetPaymentInstructions()
    {
        var settings = instructions.Value;
        return new PaymentInstructions(settings.AccountHolder, Blank(settings.YapeNumber), Blank(settings.PlinNumber),
            Blank(settings.BankName), Blank(settings.BankAccountNumber), Blank(settings.BankAccountCci));
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
