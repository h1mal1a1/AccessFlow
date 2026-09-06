using AccessFlow.Application.Clients.DTOs;

namespace AccessFlow.Application.Clients;

public interface IClientService
{
    Task<long> CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken);
    Task<ClientDto> GetClientAsync(long id, CancellationToken cancellationToken);
    Task<List<ClientDto>> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task UpdateClientAsync(long id, UpdateClientDto updateClientDto, CancellationToken cancellationToken);
    Task DeleteClientAsync(long id, CancellationToken cancellationToken);
    Task<List<ClientDto>> GetDeletedClientsAsync(int page, int pageSize, CancellationToken cancellationToken);
}