namespace AccessFlow.Application.Connections.DTOs.Payload;

public sealed record ConnectionUpdatePayload(long ConnectionId, string CurrentName, string NewName, string IdExternal);