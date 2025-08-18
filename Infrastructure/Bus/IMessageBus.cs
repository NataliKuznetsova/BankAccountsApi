namespace BankAccountsApi.Infrastructure.Bus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(string type, T message);
    }
}
