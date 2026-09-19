namespace AccessFlow.Api.Contracts.Clients;

public sealed record UpdateClientRequest(string Email, string PhoneNumber, string? Comment);
