using AccessFlow.Domain.Constants;

namespace AccessFlow.Domain.Entities;

public class OutboxMessage
{
    public long Id { get; set; }
    public required OutboxMessageType Type { get; set; }
    public required string Payload { get; set; }
    public required OutboxMessageStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}