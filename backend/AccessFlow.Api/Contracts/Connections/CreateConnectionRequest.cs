namespace AccessFlow.Api.Contracts.Connections;

public class CreateConnectionRequest
{
    public long IdClient { get; set; }
    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
}