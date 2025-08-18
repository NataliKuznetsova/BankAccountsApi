using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace BankAccountsApi.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : MbControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IConnectionFactory _rabbitFactory;
    private readonly IOutboxRepository _outboxRepository;

    public HealthController(
        ILogger<HealthController> logger,
        IConnectionFactory rabbitFactory,
        IOutboxRepository outboxRepository)
    {
        _logger = logger;
        _rabbitFactory = rabbitFactory;
        _outboxRepository = outboxRepository;
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok("Сервис жив");
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        bool rabbitOk = CheckRabbit();
        int pendingOutbox = await CountPendingOutbox();

        if (!rabbitOk)
        {
            _logger.LogWarning("Готовность: RabbitMQ недоступен.");
            return StatusCode(503, "RabbitMQ недоступен");
        }

        if (pendingOutbox > 100)
        {
            _logger.LogWarning($"Очередь Outbox большая ({pendingOutbox} сообщений)");
            return Ok($"Сервис работает, отставание Outbox: {pendingOutbox} сообщений");
        }

        _logger.LogInformation($"Все проверки пройдены. Outbox={pendingOutbox}");
        return Ok($"Сервис готов. Outbox={pendingOutbox}");
    }

    private bool CheckRabbit()
    {
        try
        {
            using var connection = _rabbitFactory.CreateConnection();
            return connection.IsOpen;
        }
        catch
        {
            return false;
        }
    }

    private async Task<int> CountPendingOutbox()
    {
        try
        {
            var events = await _outboxRepository.GetPendingEventsAsync();
            return events.Count;
        }
        catch
        {
            _logger.LogWarning("Не удалось посчитать сообщения Outbox");
            return -1;
        }
    }
}
