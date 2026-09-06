using AccessFlow.Domain.Constants;

namespace AccessFlow.Domain.Entities;

public class Client
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required ClientStatus Status { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Connection> Connections { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<BulkOperationItem> BulkOperationItems { get; set; } = [];
}