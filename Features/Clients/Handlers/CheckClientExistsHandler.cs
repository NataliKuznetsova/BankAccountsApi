using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

/// <summary>
/// Запрос проверки клиента
/// </summary>
public class CheckClientExistsHandler(IInMemoryClientStorage clientStorage) : IRequestHandler<CheckClientExistsQuery, MbResult<bool>>
{
    public Task<MbResult<bool>> Handle(CheckClientExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = clientStorage.Exists(request.Id);
        return Task.FromResult(MbResult<bool>.Success(exists));
    }
}