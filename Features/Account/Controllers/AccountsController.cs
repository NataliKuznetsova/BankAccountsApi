using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Features.Account.Controllers
{
    /// <summary>
    /// Контроллер для работы со счетами.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("accounts")]
    public class AccountsController(IMediator mediator) : MbControllerBase
    {
        /// <summary>
        /// Создание нового счёта
        /// </summary>
        /// <param name="command">Данные для создания счёта</param>
        /// <returns>Возвращает идентификатор созданного счета</returns>
        [HttpPost]
        [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status404NotFound)]
        public async Task<MbResult<Guid>> CreateAccount([FromBody] CreateAccountCommand command)
        {
            return await mediator.Send(command);
        }

        /// <summary>
        /// Получение счёта
        /// </summary>
        /// <param name="accountId">Id счёта</param>
        /// <returns>Счёт клиента</returns>
        [HttpGet("{accountId:guid}")]
        [ProducesResponseType(typeof(MbResult<Models.Account>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<Models.Account>), StatusCodes.Status404NotFound)]
        public async Task<MbResult<Models.Account>> GetAccountById(Guid accountId)
        {
            return await mediator.Send(new GetAccountByIdQuery(accountId));
        }

        /// <summary>
        /// Удаляет счет по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status404NotFound)]
        public async Task<MbResult<Unit>> DeleteAccount(Guid id)
        {
            return await mediator.Send(new DeleteAccountCommand(id));
        }

        /// <summary>
        /// Получить список всех счетов
        /// </summary>
        /// <param name="ownerId">Идентификатор пользователя</param>
        /// <returns>Список счетов</returns>
        [HttpGet("{ownerId:guid}/all")]
        [ProducesResponseType(typeof(MbResult<List<Models.Account>>), StatusCodes.Status200OK)]
        public async Task<MbResult<List<Models.Account>>> GetAccounts(Guid ownerId)
        {
            var query = new GetAccountsQuery { OwnerId = ownerId };
            return await mediator.Send(query);
        }

        /// <summary>
        /// Обновление счета
        /// </summary>
        /// <param name="id">Идентификатор счёта</param>
        /// <param name="command">Данные для обновления</param>
        /// <returns>204 NoContent</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status404NotFound)]
        public async Task<MbResult<Unit>> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
        {
            command.Id = id;
            return await mediator.Send(command);
        }

        /// <summary>
        /// Проверка на наличие счета
        /// </summary>
        /// <param name="ownerId">Идентификатор клиента</param>
        /// <returns>Результат проверки</returns>
        [HttpGet("hasaccount/{ownerId:guid}")]
        [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status200OK)]
        public async Task<MbResult<bool>> HasAccount(Guid ownerId)
        {
            return await mediator.Send(new HasAccountByOwnerIdQuery(ownerId));
        }
        /// <summary>
        /// Открытие нового вклада
        /// </summary>
        /// <param name="command">Данные для открытия вклада</param>
        /// <returns>Идентификатор созданного вклада</returns>
        [HttpPost("deposit/create")]
        [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<MbResult<Guid>> CreateDeposit([FromBody] CreateDepositCommand command)
        {
            return await mediator.Send(command);
        }

        /// <summary>
        /// Закрытие вклада с начислением процентов
        /// </summary>
        /// <param name="command">Данные для закрытия вклада</param>
        [HttpPost("deposit/close")]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status200OK)]
        public async Task<MbResult<Unit>> CloseDeposit([FromBody] CloseDepositCommand command)
        {
            return await mediator.Send(command);
        }
    }
}