using CryptoService.DTOs;

<<<<<<< HEAD
namespace CryptoService.Services.Interfaces;

public interface ICryptoPaymentService
{
    Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);
    Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    Task<string> ProcessPaymentAsync(Guid paymentId, CancellationToken cancellationToken); // add this
    Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken);
=======
namespace CryptoService.Services.Interfaces
{
    public interface ICryptoPaymentService
    {
        Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken);

        Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    }
>>>>>>> 69563e2 (Add wallet, start)
}
