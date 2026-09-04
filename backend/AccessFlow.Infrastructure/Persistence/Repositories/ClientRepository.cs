using AccessFlow.Domain.Entities;
using AccessFlow.Application.Abstractions;
using AccessFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using AccessFlow.Application.Clients.Exceptions;
using AccessFlow.Domain.Constants;

namespace AccessFlow.Infrastructure.Persistence.Repositories;

public class ClientRepository(AppDbContext dbContext) : IClientRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    public async Task<Client> GetClientAsync(long id, CancellationToken cancellationToken) =>
        await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ClientNotFoundException(id);
    public async Task<List<Client>> GetClientsByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken) =>
            await _dbContext.Clients.Where(cli => listIds.Contains(cli.Id))
                .ToListAsync(cancellationToken);

    public async Task<List<Client>> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken)
        => await _dbContext.Clients
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        await _dbContext.Clients.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateClientAsync(long id, string email, string phoneNumber, string? comment,
        CancellationToken cancellationToken)
    {
        Client client = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ClientNotFoundException(id);
        client.Comment = comment;
        client.Email = email;
        client.PhoneNumber = phoneNumber;
        client.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteClientAsync(long id, CancellationToken cancellationToken)
    {
        Client client = await _dbContext.Clients
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ClientNotFoundException(id);
        var now = DateTimeOffset.UtcNow;
        foreach (var connection in client.Connections)
        {
            connection.Status = ConnectionStatus.Deleted;
            connection.UpdatedAt = now;
        }
        client.Status = ClientStatus.Deleted;
        client.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<Client>> GetDeletedClientsAsync(CancellationToken cancellationToken) =>
        await _dbContext.Clients
                .IgnoreQueryFilters()
                .Where(x => x.Status == ClientStatus.Deleted)
                .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken) =>
        await _dbContext.Clients.AnyAsync(client => client.Id == id, cancellationToken);
}