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
    public async Task AddConnectionAsync(Connection Connection, CancellationToken cancellationToken)
    {
        await _dbContext.Connections.AddAsync(Connection, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateConnectionAsync(long id, string idExternal, string name, string connectionString,
        string subUrl, CancellationToken cancellationToken)
    {
        Connection connection = await _dbContext.Connections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ConnectionNotFoundException(id);
        connection.Name = name;
        connection.IdExternal = idExternal;
        connection.ConnectionString = connectionString;
        connection.SubUrl = subUrl;
        connection.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteConnectionAsync(long id, CancellationToken cancellationToken)
    {
        Connection client = await _dbContext.Connections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new ConnectionNotFoundException(id);
        client.Status = ConnectionStatus.Deleted;
        client.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<Connection>> GetDeletedConnectionsAsync(CancellationToken cancellationToken) =>
        await _dbContext.Connections
            .IgnoreQueryFilters()
            .Where(x => x.Status == ConnectionStatus.Deleted)
            .ToListAsync(cancellationToken);
}