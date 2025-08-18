using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для создания счета
/// </summary>
public class CreateAccountHandler(IAccountsRepository storage, 
            IClientsRepository clientStorage, 
            IOutboxRepository outboxRepository,
            AppDbContext _context) : IRequestHandler<CreateAccountCommand, MbResult<Guid>>
{
    public async Task<MbResult<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (!await clientStorage.ExistsAsync(request.OwnerId))
        {
            return MbResult<Guid>.Failure(MbError.NotFound("Клиент не найден"));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        try
        {
            var accountId = Guid.NewGuid();
            var newAccount = new Models.Account()
            {
                Id = accountId,
                OwnerId = request.OwnerId,
                Type = request.Type,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                Balance = 0,
                OpenDate = DateTime.UtcNow
            };

            await storage.CreateAsync(newAccount);

            await outboxRepository.AddEventAsync("AccountOpened", new
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                AccountId = accountId,
                request.OwnerId,
                request.Currency
            });

            await transaction.CommitAsync(cancellationToken);
            return MbResult<Guid>.Success(newAccount.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult.Failure<Guid>(MbError.Internal($"Ошибка при создании счета: {ex.Message}"));
        }
    }
}