using System.Net;
using System.Net.Http.Json;
using AccessFlow.Application.Connections.DTOs;
using AccessFlow.IntegrationTests.Clients;
using AccessFlow.IntegrationTests.Connections;
using AccessFlow.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
namespace AccessFlow.IntegrationTests.ErrorHandling;

public class ProblemDetailsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly ClientTestHelper _clientHelper;
    private readonly ConnectionTestHelper _connectionHelper;
    private static string RndStr() => Guid.NewGuid().ToString("N");

    public ProblemDetailsTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
        _clientHelper = new ClientTestHelper(_client);
        _connectionHelper = new ConnectionTestHelper(_client);
    }

    [Fact]
    public async Task GetClient_WhenClientDoesNotExist_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync($"/api/clients/{long.MaxValue}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Resource not found", problemDetails.Title);
    }

    [Fact]
    public async Task CreateConnection_WhenUniqueFieldAlreadyExists_ReturnsProblemDetails()
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
        var firstResponse = await _client.PostAsJsonAsync("/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync($"/api/connections", createConnectionDto);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var problemDetails = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Connection conflict", problemDetails.Title);
    }
}