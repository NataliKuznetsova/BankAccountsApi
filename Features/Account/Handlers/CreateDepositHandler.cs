using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers
{
    /// <summary>
    /// Хэндлер для открытия нового депозитного счёта.
    /// </summary>
    public class CreateDepositHandler(
        IAccountsRepository accountsRepository,
        IClientsRepository clientsRepository)
        : IRequestHandler<CreateDepositCommand, MbResult<Guid>>
    {
        public async Task<MbResult<Guid>> Handle(CreateDepositCommand request, CancellationToken cancellationToken)
        {
            if (!await clientsRepository.ExistsAsync(request.OwnerId))
            {
                return MbResult<Guid>.Failure(MbError.NotFound("Клиент не найден"));
            }

            var depositAccount = new Models.Account
            {
                Id = Guid.NewGuid(),
                OwnerId = request.OwnerId,
                Type = AccountType.Deposit,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                Balance = 0,
                OpenDate = DateTime.UtcNow,
                CloseDate = request.MaturityDate
            };

            await accountsRepository.CreateAsync(depositAccount);

            return MbResult<Guid>.Success(depositAccount.Id);
        }
    }
}
