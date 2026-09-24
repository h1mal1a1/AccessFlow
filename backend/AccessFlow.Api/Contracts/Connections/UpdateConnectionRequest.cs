using System.ComponentModel.DataAnnotations;

namespace AccessFlow.Api.Contracts.Connections;

public sealed record UpdateConnectionRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name
);