using BankAccountsApi.Models;
using BankAccountsApi.Storage.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Currency> Currency { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        }
    }
}
