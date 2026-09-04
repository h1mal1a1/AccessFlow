using AccessFlow.Domain.Entities;
namespace AccessFlow.Application.Abstractions;

public interface IClientRepository
{
    Task<Client> GetClientAsync(long id, CancellationToken cancellationToken);
    Task<List<Client>> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<Client>> GetClientsByIdsAsync(IReadOnlyCollection<long> listIds, CancellationToken cancellationToken);
    Task AddClientAsync(Client client, CancellationToken cancellationToken);
    Task UpdateClientAsync(long id, string email, string phoneNumber, string? comment,
        CancellationToken cancellationToken);
    Task DeleteClientAsync(long id, CancellationToken cancellationToken);
    Task<List<Client>> GetDeletedClientsAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
}