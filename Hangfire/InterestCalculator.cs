using Dapper;
using Npgsql;

namespace BankAccountsApi.Hangfire
{
    public class InterestService(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("DbConnection")!;

        public async Task CalculateInterestAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Начинаем транзакцию с уровнем изоляции Serializable
            await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Получаем все счета Deposit и Credit с ненулевой процентной ставкой
                var accounts = await connection.QueryAsync<Guid>(@"
                    SELECT ""Id""
                    FROM public.""Accounts""
                    WHERE ""Type"" IN (1, 3) 
                      AND ""InterestRate"" > 0
                ", transaction: transaction);

                foreach (var accountId in accounts)
                {
                    // Вызываем процедуру начисления процентов
                    await connection.ExecuteAsync("CALL accrue_interest(@AccountId)",
                        new { AccountId = accountId }, transaction: transaction);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
