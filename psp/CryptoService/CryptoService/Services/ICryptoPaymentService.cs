using CryptoService.DTOs;

namespace CryptoService.Services
{
    public interface ICryptoPaymentService
    {
        Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);

        Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    }
}
