using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Account.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Controllers
{
    /// <summary>
    /// Контроллер для работы со счетами.
    /// </summary>
    [ApiController]
    [Route("accounts")]
    public class AccountsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Создание нового счёта
        /// </summary>
        /// <param name="command">Данные для создания счёта</param>
        /// <returns>Возвращает идентификатор созданного счета</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
        {
            var accountId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetAccountById), new { accountId }, accountId);
        }

        /// <summary>
        /// Получение счёта
        /// </summary>
        /// <param name="accountId">Id счёта</param>
        /// <returns>Счёт клиента</returns>
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountById(Guid accountId)
        {
            var result = await mediator.Send(new GetAccountByIdQuery(accountId));
            return Ok(result);
        }
        
        /// <summary>
        /// Удаляет счет по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var command = new DeleteAccountCommand(id);
            await mediator.Send(command);
            return NoContent();
        }
        
        /// <summary>
        /// Получить список всех счетов
        /// </summary>
        /// <returns>Список счетов</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AccountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccounts()
        {
            var query = new GetAccountsQuery();
            var result = await mediator.Send(query);
            return Ok(result);
        }
        
        /// <summary>
        /// Обновление счета
        /// </summary>
        /// <param name="id">Идентификатор счёта</param>
        /// <param name="command">Данные для обновления</param>
        /// <returns>204 NoContent</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
        {
            command.Id = id;
            await mediator.Send(command);
            return NoContent();
        }
        
        /// <summary>
        /// Проверка на наличие счета
        /// </summary>
        /// <param name="ownerId">Идентификатор клиента</param>
        /// <returns>Результат проверки</returns>
        [HttpGet("hasaccount/{ownerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> HasAccount(Guid ownerId)
        {
            var result = await mediator.Send(new HasAccountByOwnerIdQuery(ownerId));
            return Ok(result);
        }
    }
}