using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Выполнить перевод между счетами.
    /// </summary>
    /// <param name="command">Данные перевода</param>
    /// <returns>Идентификатор транзакции</returns>
    /// <response code="200">Перевод успешно выполнен.</response>
    /// <response code="400">Некорректные данные перевода.</response>
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] ExecuteTransferCommand command)
    {
        return Ok(await mediator.Send(command));
    }
    
    /// <summary>
    /// Получить выписку по счёту
    /// </summary>
    /// <param name="accountId">Идентификатор счета</param>
    /// <returns>Выписка</returns>
    [HttpGet("statement/{accountId}")]
    public async Task<IActionResult> GetStatement(Guid accountId)
    {
        var result = await mediator.Send(new GetStatementQuery(accountId));
        return Ok(result);
    }
    
    /// <summary>
    /// Регистрация транзакции по счету
    /// </summary>
    /// <param name="command">Данные транзакции</param>
    /// <returns>Идентификатор транзакции</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterTransaction([FromBody] RegisterTransactionCommand command)
    {
        return Ok(await mediator.Send(command));
    }
}
