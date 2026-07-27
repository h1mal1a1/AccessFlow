namespace AccessFlow.Domain.Entities;

public class Client
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}