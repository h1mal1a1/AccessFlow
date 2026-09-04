using AccessFlow.Domain.Entities;
using AccessFlow.Application.Abstractions;
using AccessFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AccessFlow.Infrastructure.Persistence.Repositories;

public class ClientRepository(AppDbContext dbContext) : IClientRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    public async Task<Client> GetClientAsync(long id, CancellationToken cancellationToken) =>
        await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new Exception($"Client with id = {id} is not contains in Db");
    public async Task<List<Client>> GetClientsByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken) =>
            await _dbContext.Clients.Where(cli => listIds.Contains(cli.Id))
                .ToListAsync(cancellationToken);

    public async Task<List<Client>> GetClientsAsync(CancellationToken cancellationToken)
        => await _dbContext.Clients.ToListAsync(cancellationToken);
    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        await _dbContext.Clients.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateClientAsync(long id, string email, string phoneNumber, string? comment,
        CancellationToken cancellationToken)
    {
        Client client = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new Exception($"Client with id = {id} is not contains in Db");
        client.Comment = comment;
        client.Email = email;
        client.PhoneNumber = phoneNumber;
        client.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteClientAsync(long id, CancellationToken cancellationToken)
    {
        Client client = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new Exception($"Client with id = {id} is not contains in Db");
        client.Status = "Deleted";
        client.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<Client>> GetDeletedClientsAsync(CancellationToken cancellationToken) =>
        await _dbContext.Clients
                .IgnoreQueryFilters()
                .Where(x => x.Status == "Deleted")
                .ToListAsync(cancellationToken);
}