using CardService.Domain.Enums;

namespace CardService.Domain.DTOs
{
    public record BankInitPaymentRequest(
        string MerchantId,
        Guid PspPaymentId,
        double Amount,
        Currency Currency,
        string Stan,
        DateTime PspTimestamp
    );
}
