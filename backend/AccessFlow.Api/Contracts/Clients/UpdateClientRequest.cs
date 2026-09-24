using System.ComponentModel.DataAnnotations;

namespace AccessFlow.Api.Contracts.Clients;

public sealed record UpdateClientRequest(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, Phone, StringLength(30)] string PhoneNumber,
    [property: StringLength(500)] string? Comment
);
