using AccessFlow.Domain.Constants;

namespace AccessFlow.Application.Connections.DTOs;

public sealed record ConnectionDto(long Id, long IdClient, string? IdExternal, string Name, string? ConnectionString,
    string? SubUrl, ConnectionStatus Status);