using BankAccountsApi.Behaviors;
using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankAccountsApi.Tests.Integration
{
    [TestFixture]
    public class ParallelTransferTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private string _connectionString = null!;

        private Guid _accountFromId;
        private Guid _accountToId;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _connectionString = "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=mypassword";

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(x =>
                {
                    x.ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddJsonFile("appsettings.Test.json", optional: false);
                    });

                    x.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication("TestScheme")
                                .AddScheme<AuthenticationSchemeOptions, AuthTestHandler>(
                                    "TestScheme", _ => { });
                        services.AddScoped<IInboxRepository, InboxRepository>();
                        services.AddScoped<IInboxDeadLetterRepository, InboxDeadLetterRepository>();

                        services.AddScoped<IOutboxRepository, OutboxRepository>();
                    });
                });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("TestScheme");

            await PrepareDatabase();
        }

        private async Task PrepareDatabase()
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var clientId = Guid.NewGuid();
            _accountFromId = Guid.NewGuid();
            _accountToId = Guid.NewGuid();

            await conn.ExecuteAsync(
                "INSERT INTO public.\"Clients\" (\"Id\", \"Name\", \"LastName\") VALUES (@Id, @Name, @LastName)",
                new { Id = clientId, Name = "Клиент", LastName = "Один" });

            await conn.ExecuteAsync(
                "INSERT INTO public.\"Clients\" (\"Id\", \"Name\", \"LastName\") VALUES (@Id, @Name, @LastName)",
                new { Id = Guid.NewGuid(), Name = "Клиент", LastName = "Два" });

            await conn.ExecuteAsync(@"
                INSERT INTO public.""Accounts"" (""Id"", ""OwnerId"", ""Balance"", ""Currency"", ""Type"", ""InterestRate"", ""OpenDate"") 
                VALUES (@Id, @OwnerId, @Balance, @Currency, @Type, @InterestRate, @OpenDate)",
                new
                {
                    Id = _accountFromId,
                    OwnerId = clientId,
                    Balance = 1000m,
                    Currency = "RUB",
                    Type = 1,
                    InterestRate = 0m,
                    OpenDate = DateTime.UtcNow
                });

            await conn.ExecuteAsync(@"
                INSERT INTO public.""Accounts"" (""Id"", ""OwnerId"", ""Balance"", ""Currency"", ""Type"", ""InterestRate"", ""OpenDate"") 
                VALUES (@Id, @OwnerId, @Balance, @Currency, @Type, @InterestRate, @OpenDate)",
                new
                {
                    Id = _accountToId,
                    OwnerId = clientId,
                    Balance = 1000m,
                    Currency = "RUB",
                    Type = 1,
                    InterestRate = 0m,
                    OpenDate = DateTime.UtcNow
                });
        }

        [Test]
        public async Task Transfer_50ParallelRequests_TotalBalanceIsPreserved()
        {
            decimal transferAmount = 10m;
            int parallelRequests = 50;

            var initialTotal = await GetTotalBalance();

            var tasks = Enumerable.Range(0, parallelRequests)
                .Select(_ => TransferAsync(transferAmount))
                .ToArray();

            await Task.WhenAll(tasks);

            var finalTotal = await GetTotalBalance();

            Assert.That(finalTotal, Is.EqualTo(initialTotal));
        }

        private async Task TransferAsync(decimal amount)
        {
            var command = new ExecuteTransferCommand
            {
                FromAccountId = _accountFromId,
                ToAccountId = _accountToId,
                Amount = amount,
                Currency = "RUB"
            };

            var response = await _client.PostAsJsonAsync("/transactions/transfer", command);
            response.EnsureSuccessStatusCode();
        }

        private async Task<decimal> GetTotalBalance()
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<decimal>(
                "SELECT COALESCE(SUM(\"Balance\"), 0) FROM public.\"Accounts\" WHERE \"Id\" = @From OR \"Id\" = @To",
                new { From = _accountFromId, To = _accountToId });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
