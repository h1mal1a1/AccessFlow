using AccessFlow.Domain.Constants;

namespace AccessFlow.Application.Connections.DTOs;

public class ConnectionListDto
{
    public long Id { get; set; }
    public long IdClient { get; set; }
    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required ConnectionStatus Status { get; set; }
}