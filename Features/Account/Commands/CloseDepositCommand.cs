using BankAccountsApi.Infrastructure;
using MediatR;
using System;

namespace BankAccountsApi.Features.Account.Commands
{
    /// <summary>
    /// Команда для закрытия депозитного счёта и начисления процентов
    /// </summary>
    public class CloseDepositCommand : IRequest<MbResult<Unit>>
    {
        /// <summary>
        /// Идентификатор депозитного счёта
        /// </summary>
        public Guid DepositId { get; set; }

        /// <summary>
        /// Идентификатор сотрудника
        /// </summary>
        public Guid ManagerId { get; set; }
    }
}
