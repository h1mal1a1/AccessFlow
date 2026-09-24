using AccessFlow.Domain.Entities;
namespace AccessFlow.Application.Abstractions;

public interface IConnectionRepository
{
    Task<Connection> GetConnectionAsync(long id, CancellationToken cancellationToken);
    Task<List<Connection>> GetConnectionsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<Connection>> GetConnectionByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken);
    Task<List<Connection>> GetConnectionsByClientIdAsync(long clientId, CancellationToken cancellationToken);
    Task AddConnectionAsync(Connection connection, CancellationToken cancellationToken);
    Task MarkDeletingAsync(long id, CancellationToken cancellationToken);
    Task<List<Connection>> GetDeletedConnectionsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Connection> GetConnectionForUpdateAsync(long id, CancellationToken ct);
}