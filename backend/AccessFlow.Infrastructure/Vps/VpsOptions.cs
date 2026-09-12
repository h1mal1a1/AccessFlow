namespace AccessFlow.Infrastructure.Vps;

public class VpsOptions
{
    public required string BaseUrl { get; set; }
    public required string ApiToken { get; set; }
    public required string InboundRemark { get; set; }
}