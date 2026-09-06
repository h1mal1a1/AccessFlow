using AccessFlow.Application.Connections.DTOs;

namespace AccessFlow.Application.Connections;

public interface IConnectionService
{
    Task<long> CreateConnectionAsync(CreateConnectionDto createConnectionDto, CancellationToken cancellationToken);
    Task<ConnectionDto> GetConnectionAsync(long id, CancellationToken cancellationToken);
    Task<List<ConnectionListDto>> GetConnectionsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task UpdateConnectionAsync(long id, UpdateConnectionDto updateConnectionDto, CancellationToken cancellationToken);
    Task DeleteConnectionAsync(long id, CancellationToken cancellationToken);
    Task<List<ConnectionListDto>> GetDeletedConnectionsAsync(int page, int pageSize, CancellationToken cancellationToken);
}
