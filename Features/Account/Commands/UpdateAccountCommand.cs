using MediatR;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Infrastructure.Results;

namespace BankAccountsApi.Features.Account.Commands
{
    /// <summary>
    /// Команда для обновления данных счета.
    /// </summary>
    public class UpdateAccountCommand : IRequest<MbResult<Unit>>
    {
        public Guid Id { get; set; }

        public AccountType Type { get; set; }

        public string Currency { get; set; } = string.Empty;

        public decimal? InterestRate { get; set; }

        public DateTime? CloseDate { get; set; }
    }
}