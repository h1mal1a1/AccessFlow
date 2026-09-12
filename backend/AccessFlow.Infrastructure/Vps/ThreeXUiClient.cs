using AccessFlow.Application.Abstractions;
using System.Net.Http.Json;
using AccessFlow.Infrastructure.Vps.Models;
using AccessFlow.Application.VPS.Exceptions;

namespace AccessFlow.Infrastructure.Vps;

public class ThreeXUiClient(ThreeXUiHelper helper) : IVpsClient
{
    private readonly ThreeXUiHelper _helper = helper;
    public async Task<VpsConnectionInfo?> GetConnectionAsync(string name, CancellationToken cancellationToken)
    {
        ThreeXUiClientDto? clientInfo = await _helper.GetClientInfoAsync(name, cancellationToken);
        if (clientInfo is null)
            return null;
        var connectionString = await _helper.GetConnectionStringAsync(name, cancellationToken);

        return new VpsConnectionInfo()
        {
            ConnectionString = connectionString,
            IdExternal = clientInfo.Uuid,
            Name = clientInfo.Email,
            SubUrl = _helper.BuildSubUrl(clientInfo)
        };
    }

    public async Task<VpsConnectionInfo> CreateConnectionAsync(string name, CancellationToken cancellationToken)
    {
        var request = await _helper.CreateClientRequestAsync(name, cancellationToken);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "panel/api/clients/add")
        {
            Content = JsonContent.Create(request)
        };

        using var resp = await _helper.SendAsync(httpRequest, cancellationToken);

        _helper.EnsureSuccessResponse(resp);

        var result = await _helper.ReadResponseAsync<object>(resp, cancellationToken);

        if (!result.Success && result.Msg.Contains("email already in use", StringComparison.OrdinalIgnoreCase))
            throw new VpsException(VpsErrorType.Conflict, $"3X-UI client '{name}' already exists.");

        if (!result.Success)
            throw new VpsException(VpsErrorType.OperationFailed,
                $"3X-UI failed to create client '{name}': {result.Msg}");

        return await GetConnectionAsync(name, cancellationToken) ??
            throw new VpsException(VpsErrorType.InvalidResponse,
                $"3X-UI client '{name}' was created, but could not be retrieved.");
    }
}