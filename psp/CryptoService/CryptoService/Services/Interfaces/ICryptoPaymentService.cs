using CryptoService.DTOs;

namespace CryptoService.Services.Interfaces
{
    public interface ICryptoPaymentService
    {
        Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);

        Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    }
}
