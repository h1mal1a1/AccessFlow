namespace AccessFlow.Application.Clients.DTOs;

public class UpdateClientDto
{
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
}