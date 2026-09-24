namespace AccessFlow.Application.Abstractions;

public sealed record VpsConnectionInfo
{
    /// <summary>
    /// UUID in 3X-UI.
    /// </summary>
    public required string IdExternal { get; init; }

    /// <summary>
    /// client.email in 3X-UI.
    /// </summary>
    public required string Name { get; init; }

    public required string ConnectionString { get; init; }

    public required string SubUrl { get; init; }
}