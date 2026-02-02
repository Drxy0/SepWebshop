namespace Bank.Contracts;

public enum InitializePaymentResult
{
    Success,
    InvalidSignature,
    InvalidPsp,
    InvalidMerchant
}
