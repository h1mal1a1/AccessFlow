namespace AccessFlow.Application.Connections.DTOs;

public class CreateConnectionDto
{
    public long IdClient { get; set; }
    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
}