namespace AccessFlow.Api.Contracts.Clients;

public class UpdateClientRequest
{
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
}
