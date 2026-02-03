using CardService.Domain.DTOs;

namespace CardService.Application.Abstractions.Bank
{
    public interface IBankClient
    {
        Task<BankInitPaymentResponse> InitCardPaymentAsync(BankInitPaymentRequest payment, CancellationToken cancellationToken);
    }
}
