using PayPalService.DTOs;
using PayPalService.Models;

namespace PayPalService.Services.Interfaces;

public interface IPaymentService
{
    Task<InitializePaymentResponse?> CreatePaymentAsync(InitializePaymentRequest request, CancellationToken cancellationToken);
    Task<PayPalPayment?> GetByPayPalIdAsync(string paypalOrderId);
}
