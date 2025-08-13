using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _context;

    public CurrencyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string currencyCode)
    {
        return await _context.Currency.AnyAsync(c => EF.Functions.ILike(c.Code, currencyCode));
    }

    public async Task<bool> AddAsync(Currency currency)
    {
        if (!await ExistsAsync(currency.Code))
        {
            await _context.Currency.AddAsync(currency);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
