using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Storage.Interfaces;
using Dapper;
using Npgsql;

namespace BankAccountsApi.Hangfire
{
    public class InterestService(
        IAccountsRepository storage,
        IConfiguration configuration,
        IOutboxRepository outboxRepository)
    {
        private readonly string _connectionString = configuration.GetConnectionString("DbConnection")!;

        public async Task CalculateInterestAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Получаем все счета Deposit и Credit с ненулевой процентной ставкой
                var accounts = await storage.GetByTypesAsync([AccountType.Deposit, AccountType.Credit]);
                foreach (var account in accounts)
                {
                    var result = await connection.QuerySingleOrDefaultAsync<InterestAccrualResult>(
                        "SELECT * FROM accrue_interest(@account_id)",
                        new { account_id = account.Id },
                        transaction: transaction
                    );

                    if (result != null)
                    {
                        await outboxRepository.AddEventAsync("InterestAccrued", new 
                        {
                            EventId = Guid.NewGuid(),
                            OccurredAt = DateTime.UtcNow,
                            AccountId = account,
                            result.PeriodFrom,
                            result.PeriodTo,
                            result.Amount
                        });
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public record InterestAccrualResult(
            DateTime PeriodFrom,
            DateTime PeriodTo,
            decimal Amount
        );
    }
}
