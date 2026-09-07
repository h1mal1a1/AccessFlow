using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccessFlow.Api.Contracts.Connections;
using AccessFlow.Application.Connections.DTOs;
namespace AccessFlow.IntegrationTests.Connections;

public class ConnectionTestHelper(HttpClient client)
{
    private readonly HttpClient _client = client;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };

    public async Task<long> CreateConnectionAsync(long idClient, string connectionString, string idExternal,
        string name, string subUrl)
    {
        CreateConnectionDto createConnectionDto = new()
        {
            ConnectionString = connectionString,
            IdExternal = idExternal,
            IdClient = idClient,
            Name = name,
            SubUrl = subUrl
        };

        var response = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<long>();
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/connections/{id}", response.Headers.Location.PathAndQuery);
        return id;
    }

    public async Task<long> CreateConnectionAsync(CreateConnectionDto createConnectionDto)
    {
        var response = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<long>();
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/connections/{id}", response.Headers.Location.PathAndQuery);
        return id;
    }

    public async Task<ConnectionDto> GetConnectionAsync(long idConnection)
    {
        var response = await _client.GetAsync($"/api/connections/{idConnection}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var connection = await response.Content.ReadFromJsonAsync<ConnectionDto>(JsonOptions);

        Assert.NotNull(connection);

        return connection;
    }

    public async Task DeleteConnection(long id)
    {
        var resp = await _client.DeleteAsync($"api/connections/{id}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    public async Task UpdateConnectionRequest(long idConnection, UpdateConnectionDto updateConnectionDto)
    {
        var resp = await _client.PutAsJsonAsync($"/api/connections/{idConnection}", updateConnectionDto);
        Assert.NotNull(resp);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    public async Task<List<ConnectionListDto>> GetDeletedConnections(int page, int pageSize)
    {
        var resp = await _client.GetAsync($"/api/connections/deleted?page={page}&pageSize={pageSize}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var connection = await resp.Content.ReadFromJsonAsync<List<ConnectionListDto>>(JsonOptions);
        Assert.NotNull(connection);

        return connection;
    }

    public async Task<List<ConnectionListDto>> GetActiveConnectionsAsync(int page, int pageSize)
    {
        var resp = await _client.GetAsync($"/api/connections/?page={page}&pageSize={pageSize}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var connection = await resp.Content.ReadFromJsonAsync<List<ConnectionListDto>>(JsonOptions);
        Assert.NotNull(connection);

        return connection;
    }
}