using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Clients.DTOs;
using AccessFlow.Domain.Entities;

namespace AccessFlow.Application.Clients;

public class ClientService(IClientRepository repository) : IClientService
{
    private readonly IClientRepository _clientRepository = repository;
    public async Task CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        Client client = new()
        {
            Email = createClientDto.Email,
            PhoneNumber = createClientDto.PhoneNumber,
            Comment = createClientDto.Comment,
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _clientRepository.AddClientAsync(client, cancellationToken);
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
    public async Task<List<ClientDto>> GetClientsAsync(CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetClientsAsync(cancellationToken);
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
        await _clientRepository.DeleteClientAsync(id, cancellationToken);
    }
    public async Task<List<ClientDto>> GetDeletedClientsAsync(CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetDeletedClientsAsync(cancellationToken);
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