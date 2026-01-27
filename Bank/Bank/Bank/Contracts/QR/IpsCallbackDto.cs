namespace Bank.Contracts.QR;

public class IpsCallbackDto
{
    public string Reference { get; set; } = string.Empty; // STAN from QR code
    public string TransactionId { get; set; } = string.Empty; // IPS transaction ID
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public IpsPaymentStatus Status { get; set; }
    public DateTime TransactionTimestamp { get; set; }
    public string? Reason { get; set; }
    public string Signature { get; set; } = string.Empty;

    public string? PayerAccountNumber { get; set; }
    public string? PayerName { get; set; }
}
