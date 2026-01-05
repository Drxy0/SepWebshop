namespace SepWebshop.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Oders.NotFound",
        $"The order with the Id = '{orderId}' was not found");

    public static Error InvalidLeaseTime => Error.Conflict(
        "Oders.InvalidLeaseTime",
        $"The lease time specificed is invalid");
}
