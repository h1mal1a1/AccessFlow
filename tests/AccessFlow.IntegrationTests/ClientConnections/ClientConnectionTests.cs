using System.Net;
using AccessFlow.IntegrationTests.Clients;
using AccessFlow.IntegrationTests.Connections;
using AccessFlow.IntegrationTests.Infrastructure;
using AccessFlow.Domain.Constants;
using Org.BouncyCastle.Asn1;
using AccessFlow.Application.Connections.DTOs;
using System.Net.Http.Json;

namespace AccessFlow.IntegrationTests.ClientConnections;

public class ClientConnectionTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly ClientTestHelper _clientHelper;
    private readonly ConnectionTestHelper _connectionHelper;
    private static string RndStr() => Guid.NewGuid().ToString("N");
    public ClientConnectionTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
        _clientHelper = new ClientTestHelper(_client);
        _connectionHelper = new ConnectionTestHelper(_client);
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasActiveConnection_MakesConnectionUnavailable()
    {
        var idClient = await _clientHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(
            idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _clientHelper.DeleteClientAsync(idClient);
        var resp = await _client.GetAsync($"/api/connections/{idConnection}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasActiveConnections_HidesConnectionsFromActiveList()
    {
        var idClient = await _clientHelper.CreateClientAsync();
        var idCnn1 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        var idCnn2 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _clientHelper.DeleteClientAsync(idClient);
        var listActiveConnections = await _connectionHelper.GetActiveConnectionsAsync(1, 100);
        var idsActiveConnections = listActiveConnections.Select(x => x.Id);
        Assert.DoesNotContain(idsActiveConnections, id => id == idCnn1);
        Assert.DoesNotContain(idsActiveConnections, id => id == idCnn2);
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasActiveConnections_MovesConnectionsToDeletedList()
    {
        var idClient = await _clientHelper.CreateClientAsync();
        var idCnn1 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        var idCnn2 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _clientHelper.DeleteClientAsync(idClient);
        var listDeletedConnections = await _connectionHelper.GetDeletedConnections(1, 100);
        var idsDeletedConnections = listDeletedConnections.Select(x => x.Id);
        Assert.Contains(idsDeletedConnections, id => id == idCnn1);
        Assert.Contains(idsDeletedConnections, id => id == idCnn2);
        var cnn1 = listDeletedConnections.FirstOrDefault(cnn => cnn.Id == idCnn1);
        Assert.NotNull(cnn1);
        var cnn2 = listDeletedConnections.FirstOrDefault(cnn => cnn.Id == idCnn2);
        Assert.NotNull(cnn2);
        Assert.Equal(ConnectionStatus.Deleted, cnn1.Status);
        Assert.Equal(ConnectionStatus.Deleted, cnn2.Status);
    }

    [Fact]
    public async Task CreateConnection_And_DeleteClient_ConcurrentExecution_DoesNotLeaveActiveConnection()
    {
        var idClient = await _clientHelper.CreateClientAsync();
        CreateConnectionDto createConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr(),
            IdClient = idClient
        };
        var t1 = _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        var t2 = _clientHelper.DeleteClientAsync(idClient);
        await Task.WhenAll(t1, t2);

        var resp = await t1;
        Assert.Contains(resp.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.NotFound });

        var clientResponse = await _client.GetAsync($"/api/clients/{idClient}");
        Assert.Equal(HttpStatusCode.NotFound, clientResponse.StatusCode);

        var listActiveConnections = await _connectionHelper.GetActiveConnectionsAsync(1, 100);
        var idsActiveConnectionsWithSameIdClient = listActiveConnections
            .Where(connection => connection.IdClient == idClient)
            .Select(cnn => cnn.Id);
        Assert.Empty(idsActiveConnectionsWithSameIdClient);
    }
}