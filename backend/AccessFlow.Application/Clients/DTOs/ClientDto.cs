using AccessFlow.Domain.Constants;

namespace AccessFlow.Application.Clients.DTOs;

public sealed record ClientDto(long Id, string Email, string PhoneNumber, string? Comment, ClientStatus Status);