namespace AccessFlow.Infrastructure.Vps.Models;

public class ThreeXUiCreateClientRequest
{
    public required ThreeXUiCreateClientDto Client { get; set; }
    public required List<int> InboundIds { get; set; }
}