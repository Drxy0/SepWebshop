namespace SepWebshop.Domain.Orders;

public enum OrderStatus
{     
    PendingPayment,
    Processing,
    Cancelled,
    Failed,
    Completed
}