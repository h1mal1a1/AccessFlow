using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Clients.DTOs;
using AccessFlow.Domain.Entities;
using AccessFlow.Domain.Constants;
using System.Text.Json;
using AccessFlow.Application.Connections.DTOs.Payload;
namespace AccessFlow.Application.Clients;

public class ClientService(
    IClientRepository repository,
    ITransactionManager transactionManager,
    IConnectionRepository connectionRepository,
    IOutboxMessageRepository outboxMessageRepository,
    IUnitOfWork unitOfWork) : IClientService
{
    private readonly IClientRepository _clientRepository = repository;
    private readonly ITransactionManager _transactionManager = transactionManager;
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository = outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<long> CreateClientAsync(CreateClientDto createClientDto, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        Client client = new()
        {
            Email = createClientDto.Email,
            PhoneNumber = createClientDto.PhoneNumber,
            Comment = createClientDto.Comment,
            Status = ClientStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _clientRepository.AddClientAsync(client, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return client.Id;
    }
    public async Task<ClientDto> GetClientAsync(long id, CancellationToken ct)
    {
        var client = await _clientRepository.GetClientAsync(id, ct);
        return new ClientDto(client.Id, client.Email, client.PhoneNumber, client.Comment, client.Status);
    }
    public async Task<List<ClientDto>> GetClientsAsync(int page, int pageSize, CancellationToken ct)
    {
        var clients = await _clientRepository.GetClientsAsync(page, pageSize, ct);
        return [.. clients.Select(client =>
            new ClientDto(client.Id, client.Email, client.PhoneNumber, client.Comment, client.Status)
        )];
    }
    public async Task UpdateClientAsync(long id, UpdateClientDto updateClientDto, CancellationToken ct)
    {
        await _clientRepository.UpdateClientAsync(id, updateClientDto.Email, updateClientDto.PhoneNumber,
            updateClientDto.Comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
    public async Task DeleteClientAsync(long id, CancellationToken ct)
    {
        await _transactionManager.ExecuteInTransactionAsync(async cancellationToken =>
        {
            var client = await _clientRepository.GetClientForUpdateAsync(id, cancellationToken);
            var connections = await _connectionRepository.GetConnectionsByClientIdAsync(client.Id, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            foreach (var connection in connections)
            {
                connection.Status = ConnectionStatus.Deleting;
                connection.UpdatedAt = now;

                var connectionPayload = new ConnectionDeletePayload(connection.Id, connection.Name);
                var payload = JsonSerializer.Serialize(connectionPayload);
                OutboxMessage message = new()
                {
                    Status = OutboxMessageStatus.Pending,
                    Type = OutboxMessageType.ConnectionDelete,
                    Payload = payload,
                    CreatedAt = now
                };
                await _outboxMessageRepository.AddMessageAsync(message, cancellationToken);
            }
            client.Status = ClientStatus.Deleted;
            client.UpdatedAt = now;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, ct);
    }
    public async Task<List<ClientDto>> GetDeletedClientsAsync(int page, int pageSize, CancellationToken ct)
    {
        var clients = await _clientRepository.GetDeletedClientsAsync(page, pageSize, ct);
        return [.. clients.Select(client =>
            new ClientDto(client.Id, client.Email, client.PhoneNumber, client.Comment, client.Status)
        )];
    }
}