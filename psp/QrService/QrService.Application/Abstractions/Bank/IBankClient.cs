using QrService.Domain.DTOs;
using QrService.Domain.DTOs.Bank;

namespace QrService.Application.Abstractions.Bank;

public interface IBankClient
{
    Task<BankInitPaymentResponse> InitQrPaymentAsync(BankInitPaymentRequest payment, CancellationToken cancellationToken);
}
