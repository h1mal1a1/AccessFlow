namespace AccessFlow.Api.Contracts.Clients;

public sealed record CreateClientRequest(string Email, string PhoneNumber, string? Comment);
