namespace AccessFlow.Infrastructure.Vps.Models;

public sealed record ThreeXUiCreateClientRequest(ThreeXUiCreateClientDto Client, List<int> InboundIds);