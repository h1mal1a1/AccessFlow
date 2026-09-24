namespace AccessFlow.Application.Abstractions;

public interface IVpsClient
{
    Task<VpsConnectionInfo?> GetConnectionAsync(string name, CancellationToken ct);
    Task<VpsConnectionInfo> CreateConnectionAsync(string name, CancellationToken ct);
    Task DeleteConnectionAsync(string name, CancellationToken ct);
    Task<VpsConnectionInfo> RenameConnectionAsync(string currentName, string newName, CancellationToken ct);
}