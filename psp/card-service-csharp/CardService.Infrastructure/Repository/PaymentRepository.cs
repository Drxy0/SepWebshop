using Microsoft.EntityFrameworkCore;
using CardService.Domain.Contracts;
using CardService.Domain.Entities;
using CardService.Infrastructure.Data;

namespace CardService.Infrastructure.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        
        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> InitPaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
        
        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Payment payment, CancellationToken cancellationToken)
        {
            _context.Payments.Update(payment);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
