namespace AccessFlow.Domain.Entities;

public class Notification
{
    public long Id { get; set; }
    public long IdClient { get; set; }
    public long IdConnection { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset? SendAt { get; set; }
    public string? Error { get; set; }
    public Connection Connection { get; set; } = null!;
    public Client Client { get; set; } = null!;
}