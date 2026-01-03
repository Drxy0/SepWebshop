using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Users;

namespace SepWebshop.Domain.Orders;

public sealed class Order
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;

    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }

    public float TotalPrice { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
}
