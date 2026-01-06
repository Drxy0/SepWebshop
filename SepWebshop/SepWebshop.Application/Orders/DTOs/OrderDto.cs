using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.DTOs;

public class OrderDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; set; }

    public Guid CarId { get; set; }

    public Guid InsuranceId { get; set; }

    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }

    public float TotalPrice { get; set; }
    public Currency Currency { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentMethodType? PaymentMethod { get; set; }
}
