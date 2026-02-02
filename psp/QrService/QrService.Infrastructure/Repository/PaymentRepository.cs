using Microsoft.EntityFrameworkCore;
using QrService.Domain.Contracts;
using QrService.Domain.Entities;
using QrService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Infrastructure.Repository
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
            var result = await  _context.SaveChangesAsync(cancellationToken);
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
