using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Storage
{
    public class InboxDeadLetterRepository : IInboxDeadLetterRepository
    {
        private readonly AppDbContext _dbContext;

        public InboxDeadLetterRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MarkAsDeadLetterAsync(Guid messageId, string handler, string payload, string error)
        {
            var deadLetter = new InboxDeadLetter
            {
                MessageId = messageId,
                ReceivedAt = DateTime.UtcNow,
                Handler = handler,
                Payload = payload,
                Error = error
            };

            _dbContext.InboxDeadLetter.Add(deadLetter);
            await _dbContext.SaveChangesAsync();
        }
    }
}
