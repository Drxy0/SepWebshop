using CardService.Domain.Enums;

namespace CardService.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public required string MerchantId { get; set; }
        public required string MerchantPassword { get; set; }
        public required double Amount { get; set; }
        public Currency Currency { get; set; }
        public required Guid MerchantOrderId { get; set; }
        public DateTime MerchantTimestamp { get; set; }
        public bool IsProcessed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
