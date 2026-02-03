using PayPalService.DTOs;

namespace PayPalService.Services.Interfaces;

public interface IPaymentService
{
    Task<InitializePaymentResponse?> CreatePaymentAsync(InitializePaymentRequest request, CancellationToken cancellationToken);
}
