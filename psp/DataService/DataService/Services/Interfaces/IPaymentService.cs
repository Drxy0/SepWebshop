using DataService.Contracts;
using DataService.Models;

namespace DataService.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentInitializationResult> InitializePaymentAsync(InitializePaymentRequest request);
    Task<GetPaymentResponse?> GetPaymentByOrderIdAsync(Guid merchantOrderId);
}

public sealed record PaymentInitializationResult(
    string? PaymentId,
    bool IsSuccess); // TODO, nijesam siguran da li je to to 100%