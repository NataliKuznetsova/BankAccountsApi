using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage
{
    public class InboxRepository : IInboxRepository
    {
        private readonly AppDbContext _context;

        public InboxRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.InboxConsumed.AnyAsync(x => x.MessageId == id);
        }

        public async Task MarkAsConsumedAsync(Guid id)
        {
            var msg = await _context.InboxConsumed.FindAsync(id);
            if (msg != null)
            {
                msg.ConsumedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
