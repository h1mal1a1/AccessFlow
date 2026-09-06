using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Connections.DTOs;
using AccessFlow.Application.Clients.Exceptions;
using AccessFlow.Domain.Constants;
using AccessFlow.Domain.Entities;

namespace AccessFlow.Application.Connections;

public class ConnectionService(
    IConnectionRepository connectionRepository,
    IClientRepository clientRepository,
    ITransactionManager transactionManager)
    : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITransactionManager _transactionManager = transactionManager;
    public async Task<long> CreateConnectionAsync(CreateConnectionDto createConnectionDto,
        CancellationToken cancellationToken)
    {
        return await _transactionManager.ExecuteInTransactionAsync(async ct =>
        {
            await _clientRepository.GetClientForUpdateAsync(createConnectionDto.IdClient, ct);
            var now = DateTimeOffset.UtcNow;
            Connection connection = new()
            {
                ConnectionString = createConnectionDto.ConnectionString,
                IdClient = createConnectionDto.IdClient,
                IdExternal = createConnectionDto.IdExternal,
                Name = createConnectionDto.Name,
                SubUrl = createConnectionDto.SubUrl,
                Status = ConnectionStatus.Active,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _connectionRepository.AddConnectionAsync(connection, ct);
            return connection.Id;
        }, cancellationToken);
    }
    public async Task<ConnectionDto> GetConnectionAsync(long id, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetConnectionAsync(id, cancellationToken);

        return new ConnectionDto()
        {
            Id = connection.Id,
            IdClient = connection.IdClient,
            IdExternal = connection.IdExternal,
            Name = connection.Name,
            ConnectionString = connection.ConnectionString,
            SubUrl = connection.SubUrl,
            Status = connection.Status
        };
    }
    public async Task<List<ConnectionListDto>> GetConnectionsAsync(int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetConnectionsAsync(page, pageSize, cancellationToken);
        return [.. connections.Select(connection =>
            new ConnectionListDto()
            {
                Id = connection.Id,
                IdClient = connection.IdClient,
                IdExternal = connection.IdExternal,
                Name = connection.Name,
                Status = connection.Status
            }
        )];
    }
    public async Task UpdateConnectionAsync(long id, UpdateConnectionDto updateConnectionDto,
        CancellationToken cancellationToken)
    {
        await _connectionRepository.UpdateConnectionAsync(
                id,
                updateConnectionDto.IdExternal,
                updateConnectionDto.Name,
                updateConnectionDto.ConnectionString,
                updateConnectionDto.SubUrl,
                cancellationToken
        );
    }
    public async Task DeleteConnectionAsync(long id, CancellationToken cancellationToken)
    {
        await _connectionRepository.DeleteConnectionAsync(id, cancellationToken);
    }
    public async Task<List<ConnectionListDto>> GetDeletedConnectionsAsync(int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetDeletedConnectionsAsync(page, pageSize, cancellationToken);
        return [.. connections.Select(connection =>
            new ConnectionListDto()
            {
                Id = connection.Id,
                IdClient = connection.IdClient,
                IdExternal = connection.IdExternal,
                Name = connection.Name,
                Status = connection.Status
            }
        )];
    }
}