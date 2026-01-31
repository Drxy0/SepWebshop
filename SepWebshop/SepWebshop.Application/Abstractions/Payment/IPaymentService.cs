using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Abstractions.Payment;

public interface IPaymentService
{
    Task<PaymentInitializationResult> InitializePaymentAsync(
        string merchantId,
        string merchantPassword,
        double amount,
        Currency currency,
        Guid merchantOrderId,
        DateTime merchantTimestamp,
        CancellationToken cancellationToken = default);
}

public sealed record PaymentInitializationResult(
    string? PaymentId,
    bool IsSuccess); // TODO, nijesam siguran da li je to to 100%