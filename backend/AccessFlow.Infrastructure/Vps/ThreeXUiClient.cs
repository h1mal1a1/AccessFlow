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

    public async Task DeleteConnectionAsync(string name, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"panel/api/clients/del/{name}");

        using var resp = await _helper.SendAsync(request, cancellationToken);

        _helper.EnsureSuccessResponse(resp);

        var result = await _helper.ReadResponseAsync<object>(resp, cancellationToken);

        if (!result.Success && result.Msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
            throw new VpsException(VpsErrorType.NotFound, $"3X-UI client '{name}' was not found.");

        if (!result.Success)
            throw new VpsException(VpsErrorType.OperationFailed,
                $"3X-UI failed to delete client '{name}': {result.Msg}");
    }

    public async Task<VpsConnectionInfo> RenameConnectionAsync(string currentName, string newName,
        CancellationToken cancellationToken)
    {
        var body = new ThreeXUiUpdateClientDto(newName);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"panel/api/clients/update/{currentName}")
        {
            Content = JsonContent.Create(body)
        };

        using var resp = await _helper.SendAsync(request, cancellationToken);

        _helper.EnsureSuccessResponse(resp);

        var result = await _helper.ReadResponseAsync<object>(resp, cancellationToken);

        if (!result.Success && result.Msg.Contains("record not found", StringComparison.OrdinalIgnoreCase))
            throw new VpsException(VpsErrorType.NotFound, $"3X-UI client '{currentName}' was not found.");

        if (!result.Success && result.Msg.Contains("Duplicate email", StringComparison.OrdinalIgnoreCase))
            throw new VpsException(VpsErrorType.Conflict, $"3X-UI client '{newName}' already exists.");

        if (!result.Success)
            throw new VpsException(VpsErrorType.OperationFailed,
                $"3X-UI failed to update client '{currentName}': {result.Msg}");

        return await GetConnectionAsync(newName, cancellationToken) ??
            throw new VpsException(VpsErrorType.InvalidResponse,
                $"3X-UI client '{currentName}' was updated, but could not be retrieved.");
    }
}