using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Insurances;
using SepWebshop.Domain.Users;

namespace SepWebshop.Domain.Orders;

public sealed class Order
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;

    public Guid InsuranceId { get; set; }
    public Insurance Insurance { get; set; } = null!;

    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }

    public float TotalPrice { get; set; }
    public bool IsCompleted { get; set; } = false;
    public PaymentMethodType PaymentMethod { get; set; } // TODO: maybe make it get; init; ?
}
