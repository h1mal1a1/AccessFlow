using AccessFlow.Application.Clients.DTOs;

namespace AccessFlow.Application.Clients;

public interface IClientService
{
    Task CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken);
    Task<ClientDto> GetClientAsync(long id, CancellationToken cancellationToken);
    Task<List<ClientDto>> GetClientsAsync(CancellationToken cancellationToken);
    Task UpdateClientAsync(long id, UpdateClientDto updateClientDto, CancellationToken cancellationToken);
    Task DeleteClientAsync(long id, CancellationToken cancellationToken);
    Task<List<ClientDto>> GetDeletedClientsAsync(CancellationToken cancellationToken);
}