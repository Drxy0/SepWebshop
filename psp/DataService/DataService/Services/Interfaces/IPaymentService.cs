using DataService.Contracts;

namespace DataService.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentInitializationResult> InitializePaymentAsync(InitializePaymentRequest request);
}

public sealed record PaymentInitializationResult(
    string? PaymentId,
    bool IsSuccess); // TODO, nijesam siguran da li je to to 100%