namespace DataService.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public required string Name { get; set; } // Card, QR, PayPal, Crypto
        public bool IsActive { get; set; } = true;
    }
}
