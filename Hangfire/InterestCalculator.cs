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

            var sql = @"
                UPDATE public.""Accounts""
                SET ""Balance"" = ""Balance"" + ""Balance"" * ""InterestRate"" / 100
                WHERE ""InterestRate"" > 0";

            await connection.ExecuteAsync(sql);
        }
    }
}
