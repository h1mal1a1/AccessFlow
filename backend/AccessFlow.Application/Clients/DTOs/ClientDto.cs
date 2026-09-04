namespace AccessFlow.Application.Clients.DTOs;

public class ClientDto
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
    public required string Status { get; set; }
}