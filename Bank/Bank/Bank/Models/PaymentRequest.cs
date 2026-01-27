namespace Bank.Models;

public class PaymentRequest
{
    public Guid PaymentRequestId { get; set; }

    public required string MerchantId { get; set; }
    public Merchant Merchant { get; set; }

    public Guid PspId { get; set; }

    public double Amount { get; set; }

    public Currency Currency { get; set; }

    public required string Stan { get; set; }

    public DateTime PspTimestamp { get; set; }

    public PaymentRequestStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}