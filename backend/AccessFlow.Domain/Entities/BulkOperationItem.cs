namespace AccessFlow.Domain.Entities;

public class BulkOperationItem
{
    public long Id { get; set; }
    public long IdBulkOperation { get; set; }
    public long IdClient { get; set; }
    public required string Status { get; set; }
    public string? Error { get; set; }
}