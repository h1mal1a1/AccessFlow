using System.ComponentModel.DataAnnotations;

namespace AccessFlow.Api.Contracts.Connections;

public sealed record CreateConnectionRequest(
    long IdClient,
    [property: Required, StringLength(100, MinimumLength = 1)] string Name
);