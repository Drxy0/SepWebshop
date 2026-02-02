using CardService.Domain.Entities;

namespace CardService.Domain.Contracts
{
    public interface IPaymentRepository
    {
        Task<bool> InitPaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Payment payment, CancellationToken cancellationToken);
    }
}
