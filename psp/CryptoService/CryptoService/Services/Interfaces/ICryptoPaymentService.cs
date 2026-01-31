using CryptoService.DTOs;

namespace CryptoService.Services.Interfaces;

public interface ICryptoPaymentService
{
    Task<InitializeCryptoPaymentResponse?> CreatePaymentAsync(InitializeCryptoPaymentRequest request, CancellationToken cancellationToken);
    Task<CheckCryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, bool isSimulation, CancellationToken cancellationToken);
    Task<byte[]> GeneratePaymentQrCodeAsync(Guid paymentId, CancellationToken cancellationToken);
    Task<GenerateWalletResponse> GenerateShopWalletAsync(); // for initial setup only
}
