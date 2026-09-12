using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccessFlow.Application.VPS.Exceptions;
using AccessFlow.Infrastructure.Vps.Models;
using Microsoft.Extensions.Options;

namespace AccessFlow.Infrastructure.Vps;

public class ThreeXUiHelper(HttpClient httpClient, IOptions<VpsOptions> options)
{
    private readonly HttpClient _client = httpClient;
    private readonly VpsOptions _options = options.Value;
    internal async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.SendAsync(request, cancellationToken);

            if ((int)response.StatusCode >= 500)
            {
                response.Dispose();
                throw new VpsException(VpsErrorType.Unavailable, "3X-UI is temporarily unavailable.");
            }

            return response;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new VpsException(VpsErrorType.Unavailable, "3X-UI request timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new VpsException(VpsErrorType.Unavailable, "3X-UI is unavailable.", ex);
        }
    }

    internal async Task<ThreeXUiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ThreeXUiResponse<T>>(cancellationToken)
                ?? throw new VpsException(VpsErrorType.InvalidResponse, "3X-UI returned an empty response.");
        }
        catch (JsonException ex)
        {
            throw new VpsException(VpsErrorType.InvalidResponse, "3X-UI returned invalid JSON.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new VpsException(VpsErrorType.InvalidResponse, "3X-UI returned an unsupported response format.", ex);
        }
    }

    internal void EnsureSuccessResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;
        throw new VpsException(VpsErrorType.OperationFailed,
            $"3X-UI returned HTTP {(int)response.StatusCode} ({response.StatusCode}).");
    }

    internal string BuildSubUrl(ThreeXUiClientDto clientInfo)
    {
        var baseAddress = _client.BaseAddress ??
            throw new VpsException(VpsErrorType.Configuration, "HttpClient BaseAddress is not configured.");
        return $"{baseAddress.GetLeftPart(UriPartial.Authority)}/sub/{clientInfo.SubId}";
    }

    internal async Task<ThreeXUiClientDto?> GetClientInfoAsync(string name, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"panel/api/clients/get/{name}");
        using var resp = await SendAsync(request, cancellationToken);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        EnsureSuccessResponse(resp);

        var result = await ReadResponseAsync<ThreeXUiClientInfoDto>(resp, cancellationToken);

        if (!result.Success && result.Msg.Contains("record not found", StringComparison.OrdinalIgnoreCase))
            return null;

        if (!result.Success)
            throw new VpsException(VpsErrorType.OperationFailed, $"3X-UI returned an error: {result.Msg}");

        if (result.Obj?.Client is null)
            throw new VpsException(VpsErrorType.InvalidResponse,
                "3X-UI returned success response with empty client data.");

        return result.Obj.Client;
    }

    internal async Task<string> GetConnectionStringAsync(string name, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"panel/api/clients/links/{name}");
        using var resp = await SendAsync(request, cancellationToken);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new VpsException(VpsErrorType.InvalidResponse, $"3X-UI returned no links for client '{name}'.");

        EnsureSuccessResponse(resp);

        var linksResult = await ReadResponseAsync<List<string>>(resp, cancellationToken);

        if (!linksResult.Success)
            throw new VpsException(VpsErrorType.OperationFailed, $"3X-UI returned an error: {linksResult.Msg}");

        if (linksResult.Obj is null || linksResult.Obj.Count == 0)
            throw new VpsException(VpsErrorType.InvalidResponse, "3X-UI returned no connection links.");

        return linksResult.Obj[0];
    }

    internal async Task<ThreeXUiInboundDto> GetInboundAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "panel/api/inbounds/list");
        using var resp = await SendAsync(request, cancellationToken);

        EnsureSuccessResponse(resp);

        var result = await ReadResponseAsync<List<ThreeXUiInboundDto>>(resp, cancellationToken);

        if (!result.Success)
            throw new VpsException(VpsErrorType.OperationFailed, $"3X-UI returned an error: {result.Msg}");

        if (result.Obj is null)
            throw new VpsException(VpsErrorType.InvalidResponse,
                "3X-UI returned success response with empty inbound list.");

        return result.Obj.FirstOrDefault(x => x.Remark == _options.InboundRemark) ??
            throw new VpsException(VpsErrorType.Configuration,
                $"3X-UI inbound '{_options.InboundRemark}' was not found.");
    }

    internal async Task<ThreeXUiCreateClientRequest> CreateClientRequestAsync(string name,
        CancellationToken cancellationToken)
    {
        var inboundDto = await GetInboundAsync(cancellationToken);
        return new ThreeXUiCreateClientRequest()
        {
            Client = new ThreeXUiCreateClientDto
            {
                Email = name,
                TotalGB = 0,
                ExpiryTime = 0,
                TgId = 0,
                LimitIp = 0,
                Enable = true
            },
            InboundIds = [inboundDto.Id]
        };
    }
}