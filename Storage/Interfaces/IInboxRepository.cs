namespace BankAccountsApi.Storage.Interfaces
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task MarkAsConsumedAsync(Guid id);
    }
}
