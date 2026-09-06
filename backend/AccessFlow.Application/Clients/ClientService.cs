using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Clients.DTOs;
using AccessFlow.Domain.Entities;
using AccessFlow.Domain.Constants;
namespace AccessFlow.Application.Clients;

public class ClientService(
    IClientRepository repository,
    ITransactionManager transactionManager,
    IConnectionRepository connectionRepository,
    IUnitOfWork unitOfWork) : IClientService
{
    private readonly IClientRepository _clientRepository = repository;
    private readonly ITransactionManager _transactionManager = transactionManager;
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<long> CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken)
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
        await _clientRepository.AddClientAsync(client, cancellationToken);
        return client.Id;
    }
    public async Task<ClientDto> GetClientAsync(long id, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetClientAsync(id, cancellationToken);
        return new ClientDto()
        {
            Id = client.Id,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Comment = client.Comment,
            Status = client.Status
        };
    }
    public async Task<List<ClientDto>> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetClientsAsync(page, pageSize, cancellationToken);
        return [.. clients.Select(client =>
            new ClientDto()
            {
                Id = client.Id,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber,
                Comment = client.Comment,
                Status = client.Status
            }
        )];
    }
    public async Task UpdateClientAsync(long id, UpdateClientDto updateClientDto,
        CancellationToken cancellationToken)
    {
        await _clientRepository.UpdateClientAsync(id, updateClientDto.Email, updateClientDto.PhoneNumber,
            updateClientDto.Comment, cancellationToken);
    }
    public async Task DeleteClientAsync(long id, CancellationToken cancellationToken)
    {
        await _transactionManager.ExecuteInTransactionAsync(async ct =>
        {
            var client = await _clientRepository.GetClientForUpdateAsync(id, ct);
            var connections = await _connectionRepository.GetConnectionsByClientIdAsync(client.Id, ct);
            var now = DateTimeOffset.UtcNow;
            foreach (var connection in connections)
            {
                connection.Status = ConnectionStatus.Deleted;
                connection.UpdatedAt = now;
            }
            client.Status = ClientStatus.Deleted;
            client.UpdatedAt = now;
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }
    public async Task<List<ClientDto>> GetDeletedClientsAsync(int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetDeletedClientsAsync(page, pageSize, cancellationToken);
        return [.. clients.Select(client =>
            new ClientDto()
            {
                Id = client.Id,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber,
                Comment = client.Comment,
                Status = client.Status
            }
        )];
    }
}