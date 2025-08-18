using BankAccountsApi.Infrastructure.Bus;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Hangfire
{
    public class OutboxPublisher(IOutboxRepository outboxRepository, IMessageBus messageBus)
    {
        public async Task PublishPendingEventsAsync()
        {
            // Получаем все непубликованные события
            var events = await outboxRepository.GetPendingEventsAsync();

            foreach (var evt in events)
            {
                try
                {
                    await messageBus.PublishAsync(evt.Type, evt);
                    await outboxRepository.MarkAsPublishedAsync(evt.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при пуше события в очередь {evt.Id}: {ex.Message}");
                }
            }
        }
    }
}