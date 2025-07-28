using MediatR;
using System;

namespace BankAccountsApi.Features.Account.Commands
{
    /// <summary>
    /// Команда для обновления данных счета.
    /// </summary>
    public class UpdateAccountCommand : IRequest
    {
        public Guid? Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public decimal? InterestRate { get; set; }

        public DateTime? CloseDate { get; set; }
    }
}