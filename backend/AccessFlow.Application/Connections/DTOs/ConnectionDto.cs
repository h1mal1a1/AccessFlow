using AccessFlow.Domain.Constants;

namespace AccessFlow.Application.Connections.DTOs;

public class ConnectionDto
{
    public long Id { get; set; }
    public long IdClient { get; set; }
    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
    public required ConnectionStatus Status { get; set; }
}
