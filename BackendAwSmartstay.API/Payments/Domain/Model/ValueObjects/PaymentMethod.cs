namespace BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

/// <summary>How the guest paid the hotel (US-07 scenario 5).</summary>
public enum PaymentMethod
{
    /// <summary>Yape mobile wallet (operation number required).</summary>
    Yape,
    /// <summary>Plin mobile wallet (operation number required).</summary>
    Plin,
    /// <summary>Bank transfer (operation number required).</summary>
    BankTransfer,
    /// <summary>Cash at the front desk (no operation number).</summary>
    Cash,
    /// <summary>Card charged on the hotel's POS terminal (voucher/operation number required).</summary>
    CardAtFrontDesk
}
