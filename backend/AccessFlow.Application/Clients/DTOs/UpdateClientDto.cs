namespace AccessFlow.Application.Clients.DTOs;

public sealed record UpdateClientDto(string Email, string PhoneNumber, string? Comment);