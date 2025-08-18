using MediatR;
using BankAccountsApi.Infrastructure.Results;

namespace BankAccountsApi.Features.Account.Commands
{
    /// <summary>
    /// Команда для открытия нового вклада
    /// </summary>
    public class CreateDepositCommand : IRequest<MbResult<Guid>>
    {
        public Guid OwnerId { get; set; }

        public string DepositType { get; set; } = string.Empty;

        public string Currency { get; set; } = "RUB";

        public decimal InitialAmount { get; set; }

        public decimal InterestRate { get; set; }

        public DateTime OpenDate { get; set; } = DateTime.UtcNow;

        public DateTime MaturityDate { get; set; }
    }
}
