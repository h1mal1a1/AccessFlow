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

    /// <summary>
    /// Получает клиента по идентификатору с блокировкой строки FOR UPDATE. Значение id передаётся через 
    /// FromSqlInterpolated и параметризуется, поэтому не подставляется напрямую в SQL и не создаёт SQL injection.
    /// Метод должен вызываться внутри открытой транзакции.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Клиент с указанным идентификатором.</returns>
    /// <exception cref="ClientNotFoundException">
    /// Выбрасывается, если клиент с указанным id не найден.
    /// </exception>
    public async Task<Client> GetClientForUpdateAsync(long id, CancellationToken cancellationToken)
    {
        return await _dbContext.Clients
            .FromSqlInterpolated($"SELECT * FROM clients WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new ClientNotFoundException(id);
    }
    public async Task<List<Client>> GetClientsByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken) =>
            await _dbContext.Clients.Where(cli => listIds.Contains(cli.Id))
                .ToListAsync(cancellationToken);

    public async Task<List<Client>> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        await _dbContext.Clients
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
    public async Task<List<Client>> GetDeletedClientsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Clients
            .IgnoreQueryFilters()
            .Where(x => x.Status == ClientStatus.Deleted)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}