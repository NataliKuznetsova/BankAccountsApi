using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Controllers;

[ApiController]
[Route("transactions")]
[Authorize]
public class TransactionsController(IMediator mediator) : MbControllerBase
{
    /// <summary>
    /// Выполнить перевод между счетами
    /// </summary>
    /// <param name="command">Данные перевода</param>
    /// <returns>Код респонса</returns>
    /// <response code="200">Перевод успешно выполнен.</response>
    /// <response code="400">Некорректные данные перевода.</response>
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<MbResult<Unit>> Transfer([FromBody] ExecuteTransferCommand command)
    {
        return await mediator.Send(command);
    }
    
    /// <summary>
    /// Получить выписку по счёту
    /// </summary>
    /// <param name="accountId">Идентификатор счета</param>
    /// <returns>Выписка</returns>
    [HttpGet("statement/{accountId}")]
    public async Task<MbResult<List<Transaction>>> GetStatement(Guid accountId)
    {
        return await mediator.Send(new GetStatementQuery(accountId));
    }
    
    /// <summary>
    /// Регистрация транзакции по счету
    /// </summary>
    /// <param name="command">Данные транзакции</param>
    /// <returns>Идентификатор транзакции</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<MbResult<Guid>> RegisterTransaction([FromBody] RegisterTransactionCommand command)
    {
        return await mediator.Send(command);
    }
}