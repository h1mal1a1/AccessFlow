namespace AccessFlow.Infrastructure.Vps.Models;

public sealed record ThreeXUiCreateClientDto(string Email, long TotalGB, long ExpiryTime, long TgId, int LimitIp,
    bool Enable);