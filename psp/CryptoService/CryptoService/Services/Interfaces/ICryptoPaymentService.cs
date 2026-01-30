using CryptoService.DTOs;

<<<<<<< HEAD
namespace CryptoService.Services.Interfaces;

public interface ICryptoPaymentService
{
    Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);
    Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken);
<<<<<<< HEAD
<<<<<<< HEAD
=======
namespace CryptoService.Services.Interfaces
{
    public interface ICryptoPaymentService
    {
        Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);

        Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    }
>>>>>>> 69563e2 (Add wallet, start)
=======
=======
    Task<byte[]> GeneratePaymentQrCodeAsync(Guid paymentId, CancellationToken cancellationToken);
>>>>>>> b85b831 (Add qr code endpoint)
    Task<GenerateWalletResponse> GenerateShopWalletAsync(); // for initial setup
>>>>>>> 5ab45fd (Finish crypto backend)
}
