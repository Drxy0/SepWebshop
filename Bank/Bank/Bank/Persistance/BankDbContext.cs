using Bank.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.Persistance
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<DebitCard> DebitCards { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<PSP> PSPs { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.AccountNumber)
                    .IsRequired()
                    .HasColumnType("char(18)");

                entity.HasMany(a => a.DebitCards)
                    .WithOne(dc => dc.Account)
                    .HasForeignKey(dc => dc.AccountId)
                    .IsRequired();
            });

            modelBuilder.Entity<PaymentRequest>()
                .HasMany(p => p.Transactions)
                .WithOne(t => t.PaymentRequest)
                .HasForeignKey(t => t.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.Property(e => e.AccountId)
                    .IsRequired();

                entity.HasOne(e => e.Account)
                    .WithOne()
                    .HasForeignKey<Merchant>(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
