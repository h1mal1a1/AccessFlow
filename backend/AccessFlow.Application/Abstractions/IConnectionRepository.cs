using AccessFlow.Domain.Entities;
namespace AccessFlow.Application.Abstractions;

public interface IConnectionRepository
{
    Task<Connection> GetConnectionAsync(long id, CancellationToken cancellationToken);
    Task<List<Connection>> GetConnectionsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<Connection>> GetConnectionByIdsAsync(IReadOnlyCollection<long> listIds,
        CancellationToken cancellationToken);
    Task AddConnectionAsync(Connection connection, CancellationToken cancellationToken);
    Task UpdateConnectionAsync(long id, string idExternal, string name, string connectionString, string subUrl,
        CancellationToken cancellationToken);
    Task DeleteConnectionAsync(long id, CancellationToken cancellationToken);
    Task<List<Connection>> GetDeletedConnectionsAsync(CancellationToken cancellationToken);

}