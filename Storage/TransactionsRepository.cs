using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly AppDbContext _context;

    public TransactionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }
}
