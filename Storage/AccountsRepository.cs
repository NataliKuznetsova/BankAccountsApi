using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage;

public class AccountsRepository : IAccountsRepository
{
    private readonly AppDbContext _context;

    public AccountsRepository(AppDbContext context) => _context = context;

    public async Task CreateAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Account>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Accounts
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AccrueInterestAsync(Guid accountId)
    {
        var sql = "SELECT accrue_interest(@account_id)";
        await _context.Database.ExecuteSqlRawAsync(sql, new Npgsql.NpgsqlParameter("@p_accountId", accountId));
    }
    public async Task<IEnumerable<Account>> GetByTypesAsync(List<AccountType> types)
    {
        return await _context.Accounts
            .Where(x => types.Contains(x.Type))
            .ToListAsync();
    }
}
