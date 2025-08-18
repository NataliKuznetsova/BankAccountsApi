namespace BankAccountsApi.Storage.Interfaces
{
    public interface IInboxDeadLetterRepository
    {
        Task MarkAsDeadLetterAsync(Guid messageId, string handler, string payload, string error);
    }
}
