using Bank.Models;

namespace Bank.Contracts;

public class PspPaymentStatusDto
{
    public Guid PaymentRequestId { get; set; }
    public Guid PspId { get; set; }
    public Guid PspPaymentId { get; set; }
    public required string Stan { get; set; }
    public Guid? GlobalTransactionId { get; set; }
    public DateTime AcquirerTimestamp { get; set; }
    public TransactionStatus Status { get; set; }
    public required string MerchantId { get; set; }
    public DateTime PspTimestamp { get; set; }
}
