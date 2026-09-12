namespace AccessFlow.Application.Abstractions;

public interface IVpsClient
{
    Task<VpsConnectionInfo?> GetConnectionAsync(string name, CancellationToken cancellationToken);
    Task<VpsConnectionInfo> CreateConnectionAsync(string name, CancellationToken cancellationToken);
}