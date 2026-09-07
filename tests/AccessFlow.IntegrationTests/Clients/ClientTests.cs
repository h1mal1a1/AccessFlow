using System.Net;
using System.Net.Http.Json;
using AccessFlow.Application.Clients.DTOs;
using AccessFlow.Domain.Constants;
using AccessFlow.IntegrationTests.Infrastructure;

namespace AccessFlow.IntegrationTests.Clients;

public class ClientTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly ClientTestHelper _helper;

    public ClientTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
        _helper = new ClientTestHelper(_client);
    }

    [Fact]
    public async Task GetClient_WhenClientDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/clients/{long.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_WithValidData_ReturnsCreated()
    {
        await _helper.CreateClientAsync();
    }

    [Fact]
    public async Task GetClient_AfterCreated_ReturnsOk()
    {
        var id = await _helper.CreateClientAsync();

        var client = await _helper.GetClientAsync(id);

        Assert.Equal(id, client.Id);
        Assert.Equal(ClientTestHelper.DefaultEmail, client.Email);
        Assert.Equal(ClientTestHelper.DefaultPhoneNumber, client.PhoneNumber);
        Assert.Equal(ClientTestHelper.DefaultComment, client.Comment);
    }

    [Fact]
    public async Task UpdateClient_WhenClientExists_ReturnsNoContentAndUpdatesData()
    {
        var id = await _helper.CreateClientAsync();
        var updateDto = ClientTestHelper.CreateUpdateDto();

        var response = await _client.PutAsJsonAsync($"/api/clients/{id}", updateDto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var client = await _helper.GetClientAsync(id);

        Assert.Equal(updateDto.Email, client.Email);
        Assert.Equal(updateDto.PhoneNumber, client.PhoneNumber);
        Assert.Equal(updateDto.Comment, client.Comment);
    }

    [Fact]
    public async Task UpdateClient_WhenClientDoesNotExist_ReturnsNotFound()
    {
        var updateDto = ClientTestHelper.CreateUpdateDto();

        var response = await _client.PutAsJsonAsync($"/api/clients/{long.MaxValue}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_WhenClientIsDeleted_ReturnsNotFound()
    {
        var id = await _helper.CreateClientAsync();
        await _helper.DeleteClientAsync(id);

        var updateDto = ClientTestHelper.CreateUpdateDto();

        var response = await _client.PutAsJsonAsync($"/api/clients/{id}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WhenClientExists_ReturnsNoContent()
    {
        var id = await _helper.CreateClientAsync();

        var response = await _client.DeleteAsync($"/api/clients/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WhenClientDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/clients/{long.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetClient_AfterDeleted_ReturnsNotFound()
    {
        var id = await _helper.CreateClientAsync();
        await _helper.DeleteClientAsync(id);

        var response = await _client.GetAsync($"/api/clients/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedClients_AfterClientDeleted_ContainsDeletedClient()
    {
        var id = await _helper.CreateClientAsync();
        var client = await _helper.GetClientAsync(id);

        await _helper.DeleteClientAsync(id);

        var deletedClients = await _helper.GetDeletedClientsAsync();

        var deletedClient = deletedClients.FirstOrDefault(x => x.Id == id);

        Assert.NotNull(deletedClient);
        Assert.Equal(client.Email, deletedClient.Email);
        Assert.Equal(client.PhoneNumber, deletedClient.PhoneNumber);
        Assert.Equal(client.Comment, deletedClient.Comment);
    }

    [Fact]
    public async Task GetDeletedClients_AfterClientDeleted_HasDeletedStatus()
    {
        var id = await _helper.CreateClientAsync();

        await _helper.DeleteClientAsync(id);

        var deletedClients = await _helper.GetDeletedClientsAsync();

        var deletedClient = deletedClients.FirstOrDefault(x => x.Id == id);

        Assert.NotNull(deletedClient);
        Assert.Equal(ClientStatus.Deleted, deletedClient.Status);
    }

    [Fact]
    public async Task GetClients_WhenActiveClientsExist_ContainsActiveClient()
    {
        var id = await _helper.CreateClientAsync();
        var listClients = await _helper.GetActiveClientsAsync();
        var foundClient = listClients.FirstOrDefault(cli => cli.Id == id);
        Assert.NotNull(foundClient);
        Assert.Equal(ClientStatus.Active, foundClient.Status);
    }

    [Fact]
    public async Task GetClients_WhenClientIsDeleted_DoesNotContainDeletedClient()
    {
        var id = await _helper.CreateClientAsync();
        await _helper.DeleteClientAsync(id);
        var listActiveClients = await _helper.GetActiveClientsAsync();
        var foundClient = listActiveClients.FirstOrDefault(cli => cli.Id == id);
        Assert.Null(foundClient);
    }

    [Fact]
    public async Task GetClients_WithPagination_ReturnsRequestedPageSize()
    {
        await _helper.CreatingALimitedNumberOfClients(5);
        var activeClients = await _helper.GetActiveClientsWithParametersPageAsync(2, 2);
        Assert.Equal(2, activeClients.Count);
    }

    [Fact]
    public async Task GetClients_WithPagination_ReturnsDifferentPages()
    {
        await _helper.CreatingALimitedNumberOfClients(4);
        var activeClientsFromPage1 = await _helper.GetActiveClientsWithParametersPageAsync(1, 2);
        var idActiveClientsFromPage1 = activeClientsFromPage1.Select(x => x.Id);
        var activeClientsFromPage2 = await _helper.GetActiveClientsWithParametersPageAsync(2, 2);
        var idActiveClientsFromPage2 = activeClientsFromPage2.Select(x => x.Id);
        Assert.Empty(idActiveClientsFromPage1.Intersect(idActiveClientsFromPage2));
    }

    [Fact]
    public async Task GetClients_WithPagination_ReturnsOrderedById()
    {
        await _helper.CreatingALimitedNumberOfClients(6);
        var activeClients = await _helper.GetActiveClientsWithParametersPageAsync(1, 100);
        Assert.Equal(
            activeClients.Select(x => x.Id).OrderBy(x => x),
            activeClients.Select(x => x.Id));
    }

    [Fact]
    public async Task CreateClient_WithValidData_ReturnsLocationHeader()
    {
        for (int i = 1; i < 6; i++)
        {
            ClientDto clientDto = new()
            {
                Email = $"email{i}",
                PhoneNumber = $"+7999999999{i}",
                Comment = null,
                Status = ClientStatus.Active
            };
            var respCreate = await _client.PostAsJsonAsync("/api/clients", clientDto);
            var id = await respCreate.Content.ReadFromJsonAsync<long>();
            Assert.NotNull(respCreate.Headers.Location);
            Assert.Equal($"/api/clients/{id}", respCreate.Headers.Location.PathAndQuery);
        }
    }
}