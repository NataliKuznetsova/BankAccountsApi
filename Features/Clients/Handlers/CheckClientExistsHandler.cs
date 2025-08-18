using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

/// <summary>
/// Запрос проверки клиента
/// </summary>
public class CheckClientExistsHandler(IClientsRepository clientStorage) : IRequestHandler<CheckClientExistsQuery, MbResult<bool>>
{
    public async Task<MbResult<bool>> Handle(CheckClientExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await clientStorage.ExistsAsync(request.Id);
        return MbResult<bool>.Success(exists);
    }
}