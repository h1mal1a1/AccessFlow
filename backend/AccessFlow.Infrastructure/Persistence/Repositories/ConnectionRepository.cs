using AccessFlow.Application.Abstractions;
using AccessFlow.Infrastructure.Persistence.Data;
using AccessFlow.Domain.Entities;
using AccessFlow.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using AccessFlow.Application.Connections.Exceptions;

namespace AccessFlow.Infrastructure.Persistence.Repositories;

public class ConnectionRepository(AppDbContext dbContext) : IConnectionRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    public async Task<Connection> GetConnectionAsync(long id, CancellationToken cancellationToken) =>
        await _dbContext.Connections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ConnectionNotFoundException(id);
    public async Task<List<Connection>> GetConnectionsAsync(int page, int pageSize,
        CancellationToken cancellationToken) =>
            await _dbContext.Connections
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
    public async Task<List<Connection>> GetConnectionByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken) =>
            await _dbContext.Connections.Where(connection => listIds.Contains(connection.Id))
                .ToListAsync(cancellationToken);
    public async Task<List<Connection>> GetConnectionsByClientIdAsync(long clientId,
        CancellationToken cancellationToken) =>
            await _dbContext.Connections.Where(connection => connection.IdClient == clientId)
                .ToListAsync(cancellationToken);
    public async Task AddConnectionAsync(Connection connection, CancellationToken cancellationToken)
    {
        await _dbContext.Connections.AddAsync(connection, cancellationToken);
    }
    public async Task MarkDeletingAsync(long id, CancellationToken cancellationToken)
    {
        Connection connection = await _dbContext.Connections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ConnectionNotFoundException(id);
        connection.Status = ConnectionStatus.Deleting;
        connection.UpdatedAt = DateTimeOffset.UtcNow;
    }
    public async Task<List<Connection>> GetDeletedConnectionsAsync(int page, int pageSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Connections
            .IgnoreQueryFilters()
            .Where(x => x.Status == ConnectionStatus.Deleted)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Connection> GetConnectionForUpdateAsync(long id, CancellationToken ct)
    {
        return await _dbContext.Connections.FromSqlInterpolated($"SELECT * FROM connections WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(ct) ??
            throw new ConnectionNotFoundException(id);
    }
}