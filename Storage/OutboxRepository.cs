using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAccountsApi.Storage
{
    public class OutboxRepository(AppDbContext context) : IOutboxRepository
    {
        public async Task AddEventAsync(string eventType, object payload)
        {
            var message = new OutboxMessage
            {
                Type = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow,
                IsPublished = false
            };

            context.OutboxMessages.Add(message);
            await context.SaveChangesAsync();
        }
        public async Task<List<OutboxMessage>> GetPendingEventsAsync()
        {
            return await context.OutboxMessages
                .Where(x => !x.IsPublished && !x.IsFailed)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsPublishedAsync(Guid eventId)
        {
            var evt = await context.OutboxMessages.FindAsync(eventId);
            if (evt != null)
            {
                evt.IsPublished = true;
                await context.SaveChangesAsync();
            }
        }
        public async Task MarkAsFailedAsync(Guid eventId, string reason)
        {
            var evt = await context.OutboxMessages.FindAsync(eventId);
            if (evt != null)
            {
                evt.IsFailed = true;
                evt.FailureReason = reason;
                evt.FailedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }
}
