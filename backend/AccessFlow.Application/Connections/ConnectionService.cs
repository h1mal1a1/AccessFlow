using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Connections.DTOs;
using AccessFlow.Application.Connections.DTOs.Payload;
using AccessFlow.Domain.Constants;
using AccessFlow.Domain.Entities;
using System.Text.Json;

namespace AccessFlow.Application.Connections;

public class ConnectionService(
    IConnectionRepository connectionRepository,
    IClientRepository clientRepository,
    ITransactionManager transactionManager,
    IOutboxMessageRepository outboxMessageRepository)
    : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITransactionManager _transactionManager = transactionManager;
    private readonly IOutboxMessageRepository _outboxMessageRepository = outboxMessageRepository;
    public async Task<long> CreateConnectionAsync(CreateConnectionDto createConnectionDto, CancellationToken ct)
    {
        return await _transactionManager.ExecuteInTransactionAsync(async cancellationToken =>
        {
            await _clientRepository.GetClientForUpdateAsync(createConnectionDto.IdClient, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            Connection connection = new()
            {
                ConnectionString = null,
                IdClient = createConnectionDto.IdClient,
                IdExternal = null,
                Name = createConnectionDto.Name,
                SubUrl = null,
                Status = ConnectionStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _connectionRepository.AddConnectionAsync(connection, cancellationToken);
            var createPayload = new ConnectionCreatePayload(connection.Name, connection.Id);
            var payload = JsonSerializer.Serialize(createPayload);
            OutboxMessage outboxMessage = new()
            {
                Type = OutboxMessageType.ConnectionCreate,
                Status = OutboxMessageStatus.Pending,
                Payload = payload,
                CreatedAt = now
            };
            await _outboxMessageRepository.AddMessageAsync(outboxMessage, cancellationToken);

            return connection.Id;


        }, ct);
    }
    public async Task<ConnectionDto> GetConnectionAsync(long id, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetConnectionAsync(id, cancellationToken);

        return new ConnectionDto(connection.Id, connection.IdClient, connection.IdExternal, connection.Name,
            connection.ConnectionString, connection.SubUrl, connection.Status);
    }
    public async Task<List<ConnectionListDto>> GetConnectionsAsync(int page, int pageSize, CancellationToken ct)
    {
        var connections = await _connectionRepository.GetConnectionsAsync(page, pageSize, ct);
        return [.. connections.Select(connection =>
            new ConnectionListDto(connection.Id, connection.IdClient, connection.IdExternal, connection.Name,
                connection.Status)
        )];
    }
    public async Task UpdateConnectionAsync(long id, UpdateConnectionDto updateConnectionDto, CancellationToken ct)
    {
        await _transactionManager.ExecuteInTransactionAsync(async cancellationToken =>
        {
            var connection = await _connectionRepository.GetConnectionAsync(id, cancellationToken);
            if (connection.Status != ConnectionStatus.Active)
                throw new InvalidOperationException(
                    $"Connection '{id}' must be Active to update.");

            if (connection.Name == updateConnectionDto.Name)
                return;

            if (connection.IdExternal is null)
                throw new InvalidOperationException(
                    $"Active connection '{id}' has no external id.");
            var updatePayload = new ConnectionUpdatePayload(
                connection.Id, connection.Name, updateConnectionDto.Name, connection.IdExternal);
            var payload = JsonSerializer.Serialize(updatePayload);

            OutboxMessage message = new()
            {
                Type = OutboxMessageType.ConnectionUpdate,
                Status = OutboxMessageStatus.Pending,
                Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _outboxMessageRepository.AddMessageAsync(message, cancellationToken);
        }, ct);
    }
    public async Task DeleteConnectionAsync(long id, CancellationToken ct)
    {
        await _transactionManager.ExecuteInTransactionAsync(async cancellationToken =>
        {
            var connection = await _connectionRepository.GetConnectionAsync(id, cancellationToken);

            if (connection.Status is not (ConnectionStatus.Active or ConnectionStatus.Pending))
                throw new InvalidOperationException(
                    $"Connection '{id}' cannot be deleted in status '{connection.Status}'.");

            var deletePayload = new ConnectionDeletePayload(connection.Id, connection.Name);
            var payload = JsonSerializer.Serialize(deletePayload);
            OutboxMessage message = new()
            {
                Type = OutboxMessageType.ConnectionDelete,
                Status = OutboxMessageStatus.Pending,
                Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _outboxMessageRepository.AddMessageAsync(message, cancellationToken);
            await _connectionRepository.MarkDeletingAsync(id, cancellationToken);
        }, ct);
    }
    public async Task<List<ConnectionListDto>> GetDeletedConnectionsAsync(int page, int pageSize, CancellationToken ct)
    {
        var connections = await _connectionRepository.GetDeletedConnectionsAsync(page, pageSize, ct);
        return [.. connections.Select(connection =>
            new ConnectionListDto(connection.Id, connection.IdClient, connection.IdExternal, connection.Name,
                connection.Status)
        )];
    }
}