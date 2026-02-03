using CardService.Domain.Enums;

namespace CardService.Domain.DTOs
{
    public record DataServicePaymentResponse(
        Guid Id,
        string MerchantId,
        string MerchantPassword,
        double Amount,
        Currency Currency,
        string MerchantOrderId,
        DateTime MerchantTimestamp
    );
}
