namespace AccessFlow.Infrastructure.Vps.Models;

public class ThreeXUiCreateClientDto
{
    public required string Email { get; set; }
    public long TotalGB { get; set; }
    public long ExpiryTime { get; set; }
    public long TgId { get; set; }
    public int LimitIp { get; set; }
    public bool Enable { get; set; }
}