namespace AccessFlow.Application.Clients.DTOs;

public sealed record CreateClientDto(string Email, string PhoneNumber, string? Comment);