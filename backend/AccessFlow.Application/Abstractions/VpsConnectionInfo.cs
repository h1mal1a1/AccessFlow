namespace AccessFlow.Application.Abstractions;

public class VpsConnectionInfo
{
    /// <summary>
    /// UUID in 3x-ui
    /// </summary>
    public required string IdExternal { get; set; }      // UUID в 3X-UI
    /// <summary>
    /// client.Email in 3x-ui
    /// </summary>
    public required string Name { get; set; }            // client.email в 3X-UI
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
}