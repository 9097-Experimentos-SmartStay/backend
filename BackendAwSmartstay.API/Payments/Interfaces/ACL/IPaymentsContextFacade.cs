namespace BackendAwSmartstay.API.Payments.Interfaces.ACL;

/// <summary>How a guest can pay, exposed to other bounded contexts (e.g. the Bookings e-mails).</summary>
public sealed record PaymentInstructions(
    string AccountHolder,
    string? YapeNumber,
    string? PlinNumber,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountCci);

/// <summary>Anti-corruption layer facade of the Payments bounded context.</summary>
public interface IPaymentsContextFacade
{
    PaymentInstructions GetPaymentInstructions();
}
