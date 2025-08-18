using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddEventAsync(string eventType, object payload);
        Task<List<OutboxMessage>> GetPendingEventsAsync();
        Task MarkAsPublishedAsync(Guid eventId);
        Task MarkAsFailedAsync(Guid eventId, string reason);
    }
}
