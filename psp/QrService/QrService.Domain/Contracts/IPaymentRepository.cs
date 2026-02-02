using QrService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Domain.Contracts
{
    public interface IPaymentRepository
    {
        Task<bool> InitPaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Payment payment, CancellationToken cancellationToken);
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
