using AccessFlow.Domain.Constants;

namespace AccessFlow.Application.Connections.DTOs;

public sealed record ConnectionListDto(long Id, long IdClient, string? IdExternal, string Name,
    ConnectionStatus Status);