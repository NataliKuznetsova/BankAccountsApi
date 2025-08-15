using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Storage;

public class ClientsRepository : IClientsRepository
{
    private readonly AppDbContext _context;

    public ClientsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Client client)
    {
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid clientId)
    {
        return await _context.Clients.AnyAsync(c => c.Id == clientId);
    }

    public async Task<Client?> GetByIdAsync(Guid clientId)
    {
        return await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
    }

    public async Task UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }
}
