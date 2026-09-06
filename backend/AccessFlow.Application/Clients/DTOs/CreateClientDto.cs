namespace AccessFlow.Application.Clients.DTOs;

public class CreateClientDto
{
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
}
