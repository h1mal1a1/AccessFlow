namespace AccessFlow.Domain.Entities;

public class BulkOperation
{
    public long Id { get; set; }
    public required string Status { get; set; }
    public int UsersProcessed { get; set; }
    public int SuccessfullyProcessed { get; set; }
    public int ErrorProcessed { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
}