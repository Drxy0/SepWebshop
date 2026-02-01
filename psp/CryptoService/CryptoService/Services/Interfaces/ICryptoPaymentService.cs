using CryptoService.DTOs;

namespace CryptoService.Services.Interfaces;

public interface ICryptoPaymentService
{
    Task<byte[]?> CreatePaymentAsync(InitializeCryptoPaymentRequest request, CancellationToken cancellationToken);
    Task<CheckPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, bool isSimulation, CancellationToken cancellationToken);
    Task<GenerateWalletResponse> GenerateShopWalletAsync(); // for initial setup only
}
