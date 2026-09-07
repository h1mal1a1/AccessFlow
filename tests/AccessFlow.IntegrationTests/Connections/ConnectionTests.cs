using AccessFlow.IntegrationTests.Clients;
using AccessFlow.IntegrationTests.Infrastructure;
using AccessFlow.Domain.Constants;
using Renci.SshNet.Sftp;
using System.Net;
using AccessFlow.Application.Connections.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccessFlow.IntegrationTests.Connections;

public class ConnectionTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly ClientTestHelper _clientTestsHelper;
    private readonly ConnectionTestHelper _connectionHelper;

    private static string RndStr() => Guid.NewGuid().ToString("N");
    public ConnectionTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
        _clientTestsHelper = new ClientTestHelper(_client);
        _connectionHelper = new ConnectionTestHelper(_client);
    }

    [Fact]
    public async Task CreateConnection_WithValidData_ReturnsCreated()
    {
        var id = await _clientTestsHelper.CreateClientAsync();
        await _connectionHelper.CreateConnectionAsync(id, RndStr(), RndStr(), RndStr(), RndStr());
    }

    /// <summary>
    /// Создать Client → POST Connection → проверить 201, id > 0, Location = /api/connections/{id}.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateConnection_WithValidData_ReturnsCreatedWithIdAndLocation()
    {
        var id = await _clientTestsHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(id, RndStr(), RndStr(), RndStr(), RndStr());
        Assert.True(idConnection > 0);
    }

    /// <summary>
    /// Создать Connection → GET по id → проверить все поля и Status == Active.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetConnection_AfterCreated_ReturnsCorrectDataAndActiveStatus()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        string connectionString = RndStr();
        string idExternal = RndStr();
        string name = RndStr();
        string subUrl = RndStr();
        var idConnection = await _connectionHelper.CreateConnectionAsync(idClient, connectionString, idExternal, name,
            subUrl);
        var connection = await _connectionHelper.GetConnectionAsync(idConnection);
        Assert.Equal(connectionString, connection.ConnectionString);
        Assert.Equal(idExternal, connection.IdExternal);
        Assert.Equal(name, connection.Name);
        Assert.Equal(subUrl, connection.SubUrl);
        Assert.Equal(ConnectionStatus.Active, connection.Status);
    }

    /// <summary>
    /// Создать Connection с IdClient = long.MaxValue → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateConnection_WhenClientDoesNotExist_ReturnsNotFound()
    {
        CreateConnectionDto createConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr(),
            IdClient = long.MaxValue
        };
        var resp = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Создать Client → удалить Client → POST Connection → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateConnection_WhenClientIsDeleted_ReturnsNotFound()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        await _clientTestsHelper.DeleteClientAsync(idClient);
        CreateConnectionDto createConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr(),
            IdClient = idClient
        };
        var resp = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// GET по long.MaxValue → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetConnection_WhenConnectionDoesNotExist_ReturnsNotFound()
    {
        var resp = await _client.GetAsync($"/api/connections/{long.MaxValue}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// создать Client -> создать Connection → PUT → 204 → GET по id → проверить новые значения.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UpdateConnection_WhenConnectionExists_ReturnsNoContentAndUpdatesData()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(
            idClient, RndStr(), RndStr(), RndStr(), RndStr());
        UpdateConnectionDto updateConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr()
        };
        await _connectionHelper.UpdateConnectionRequest(idConnection, updateConnectionDto);
        var connection = await _connectionHelper.GetConnectionAsync(idConnection);
        Assert.Equal(updateConnectionDto.ConnectionString, connection.ConnectionString);
        Assert.Equal(updateConnectionDto.IdExternal, connection.IdExternal);
        Assert.Equal(updateConnectionDto.Name, connection.Name);
        Assert.Equal(updateConnectionDto.SubUrl, connection.SubUrl);
    }

    /// <summary>
    /// PUT по несуществующему id → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UpdateConnection_WhenConnectionDoesNotExist_ReturnsNotFound()
    {
        UpdateConnectionDto updateConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr()
        };
        var resp = await _client.PutAsJsonAsync($"/api/connections/{long.MaxValue}", updateConnectionDto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Создать → удалить → PUT → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UpdateConnection_WhenConnectionIsDeleted_ReturnsNotFound()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(
            idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _connectionHelper.DeleteConnection(idConnection);
        UpdateConnectionDto updateConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = RndStr(),
            Name = RndStr(),
            SubUrl = RndStr()
        };
        var resp = await _client.PutAsJsonAsync($"/api/connections/{idConnection}", updateConnectionDto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Создать → DELETE → 204 → обычный GET → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteConnection_WhenConnectionExists_ReturnsNoContentAndBecomesUnavailable()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(
            idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _connectionHelper.DeleteConnection(idConnection);

        var response = await _client.GetAsync($"/api/connections/{idConnection}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// DELETE несуществующего id → 404.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteConnection_WhenConnectionDoesNotExist_ReturnsNotFound()
    {
        var resp = await _client.DeleteAsync($"/api/connections/{long.MaxValue}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    /// <summary>
    /// Создать → удалить → GET /api/connections/deleted → найти по id → проверить Status == Deleted.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetDeletedConnections_AfterConnectionDeleted_ContainsDeletedConnection()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idConnection = await _connectionHelper.CreateConnectionAsync(
            idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _connectionHelper.DeleteConnection(idConnection);
        var listDeleted = await _connectionHelper.GetDeletedConnections(1, 100);
        var deletedCon = listDeleted.FirstOrDefault(con => con.Id == idConnection);
        Assert.NotNull(deletedCon);
        Assert.Equal(ConnectionStatus.Deleted, deletedCon.Status);
    }

    /// <summary>
    /// [Theory]: отдельно прогнать дубликаты IdExternal, Name, SubUrl → 409.
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData("IdExternal")]
    [InlineData("Name")]
    [InlineData("SubUrl")]
    public async Task CreateConnection_WithDuplicateUniqueField_ReturnsConflict(string field)
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var connectionString = RndStr();
        var idExternal = RndStr();
        var name = RndStr();
        var subUrl = RndStr();
        await _connectionHelper.CreateConnectionAsync(idClient, connectionString, idExternal, name, subUrl);
        CreateConnectionDto createConnectionDto = new()
        {
            IdClient = idClient,
            ConnectionString = RndStr(),
            IdExternal = field == "IdExternal" ? idExternal : RndStr(),
            Name = field == "Name" ? name : RndStr(),
            SubUrl = field == "SubUrl" ? subUrl : RndStr()
        };
        var resp = await _client.PostAsJsonAsync($"/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    /// <summary>
    /// [Theory]: создать две Connection → попытаться UPDATE второй значением первой → 409.
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData("IdExternal")]
    [InlineData("Name")]
    [InlineData("SubUrl")]
    public async Task UpdateConnection_WithDuplicateUniqueField_ReturnsConflict(string field)
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idConn1 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        var idConn2 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());

        var cnn1 = await _connectionHelper.GetConnectionAsync(idConn1);
        UpdateConnectionDto updateConnectionDto = new()
        {
            ConnectionString = RndStr(),
            IdExternal = field == "IdExternal" ? cnn1.IdExternal : RndStr(),
            Name = field == "Name" ? cnn1.Name : RndStr(),
            SubUrl = field == "SubUrl" ? cnn1.SubUrl : RndStr()
        };
        var resp = await _client.PutAsJsonAsync($"/api/connections/{idConn2}", updateConnectionDto);
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    /// <summary>
    /// [Theory]: создать → удалить → повторно использовать IdExternal / Name / SubUrl → 201.
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData("IdExternal")]
    [InlineData("Name")]
    [InlineData("SubUrl")]
    public async Task CreateConnection_WithSameUniqueFieldAfterSoftDelete_ReturnsCreated(string field)
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var connectionString = RndStr();
        var idExternal = RndStr();
        var name = RndStr();
        var subUrl = RndStr();

        CreateConnectionDto createConnectionDto = new()
        {
            IdClient = idClient,
            ConnectionString = connectionString,
            IdExternal = field == "IdExternal" ? idExternal : RndStr(),
            Name = field == "Name" ? name : RndStr(),
            SubUrl = field == "SubUrl" ? subUrl : RndStr()
        };
        var idConn1 = await _connectionHelper.CreateConnectionAsync(idClient, connectionString,
            idExternal, name, subUrl);
        await _connectionHelper.DeleteConnection(idConn1);
        var response = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>
    /// Создать две Connection → одну удалить → обычный список содержит активную и не содержит удалённую.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetConnections_ReturnsOnlyActiveConnections()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        var idCon1 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        var idCon2 = await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        await _connectionHelper.DeleteConnection(idCon2);
        var listConnections = await _connectionHelper.GetActiveConnectionsAsync(1, 100);
        var con1 = listConnections.FirstOrDefault(x => x.Id == idCon1);
        Assert.NotNull(con1);
        var con2 = listConnections.FirstOrDefault(x => x.Id == idCon2);
        Assert.Null(con2);
    }

    /// <summary>
    /// Создать несколько → проверить pageSize, разные страницы и порядок по Id. Это можно сделать одним тестом.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetConnections_WithPagination_ReturnsCorrectPages()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();
        for (int i = 0; i < 6; i++)
            await _connectionHelper.CreateConnectionAsync(idClient, RndStr(), RndStr(), RndStr(), RndStr());
        var listConnectionsFromPage1 = await _connectionHelper.GetActiveConnectionsAsync(1, 2);
        var listConnectionsFromPage2 = await _connectionHelper.GetActiveConnectionsAsync(2, 2);
        Assert.Equal(2, listConnectionsFromPage1.Count);
        Assert.Equal(2, listConnectionsFromPage2.Count);
        Assert.Empty(
                listConnectionsFromPage1.Select(x => x.Id)
                    .Intersect(listConnectionsFromPage2.Select(x => x.Id)));
        Assert.True(listConnectionsFromPage1.Select(x => x.Id)
            .SequenceEqual(listConnectionsFromPage1.Select(x => x.Id).OrderBy(x => x)));

        Assert.True(listConnectionsFromPage2.Select(x => x.Id)
            .SequenceEqual(listConnectionsFromPage2.Select(x => x.Id).OrderBy(x => x)));
        Assert.True(listConnectionsFromPage1.Last().Id < listConnectionsFromPage2.First().Id);
    }

    /// <summary>
    /// Проверить: список active — нет ConnectionString, SubUrl;
    /// список deleted — тоже нет;
    /// GET по id — оба поля есть.
    /// </summary>
    [Fact]
    public async Task GetConnectionLists_DoNotExposeSensitiveData_ButGetByIdDoes()
    {
        var idClient = await _clientTestsHelper.CreateClientAsync();

        var connectionString = RndStr();
        var subUrl = RndStr();

        var idConnection = await _connectionHelper.CreateConnectionAsync(idClient, connectionString, RndStr(),
            RndStr(), subUrl);

        // Active list
        var activeResponse = await _client.GetAsync("/api/connections?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);

        var activeJson = await activeResponse.Content.ReadAsStringAsync();

        using (var document = JsonDocument.Parse(activeJson))
        {
            var connection = document.RootElement
                .EnumerateArray()
                .First(x => x.GetProperty("id").GetInt64() == idConnection);

            Assert.False(connection.TryGetProperty("connectionString", out _));
            Assert.False(connection.TryGetProperty("subUrl", out _));
        }

        // GET by id
        var connectionById = await _connectionHelper.GetConnectionAsync(idConnection);

        Assert.Equal(connectionString, connectionById.ConnectionString);
        Assert.Equal(subUrl, connectionById.SubUrl);

        // Deleted list
        await _connectionHelper.DeleteConnection(idConnection);

        var deletedResponse = await _client.GetAsync("/api/connections/deleted?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, deletedResponse.StatusCode);

        var deletedJson = await deletedResponse.Content.ReadAsStringAsync();

        using (var document = JsonDocument.Parse(deletedJson))
        {
            var connection = document.RootElement
                .EnumerateArray()
                .First(x => x.GetProperty("id").GetInt64() == idConnection);

            Assert.False(connection.TryGetProperty("connectionString", out _));
            Assert.False(connection.TryGetProperty("subUrl", out _));
        }
    }

}