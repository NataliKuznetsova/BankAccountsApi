using BankAccountsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccountsApi.Storage.Configuration
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder
            .Property<byte[]>("xmin")
            .IsRowVersion()
            .IsConcurrencyToken();
        }
    }
}
