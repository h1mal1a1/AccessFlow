using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccessFlow.Application.Clients.DTOs;
using Org.BouncyCastle.Asn1.Misc;

namespace AccessFlow.IntegrationTests.Clients;

public class ClientTestHelper(HttpClient client)
{
    public const string DefaultEmail = "integration-test@example.com";
    public const string DefaultPhoneNumber = "+79999999999";
    public const string DefaultComment = "Integration test";

    private readonly HttpClient _client = client;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };

    public async Task<long> CreateClientAsync()
    {
        var request = new
        {
            Email = DefaultEmail,
            PhoneNumber = DefaultPhoneNumber,
            Comment = DefaultComment
        };

        var response = await _client.PostAsJsonAsync("/api/clients", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<long>();
    }

    public async Task<long> CreateClientWithParameters(string email, string phoneNumber, string? comment)
    {
        var request = new
        {
            Email = email,
            PhoneNumber = phoneNumber,
            Comment = comment
        };

        var response = await _client.PostAsJsonAsync("/api/clients", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<long>();
    }

    public async Task<ClientDto> GetClientAsync(long id)
    {
        var response = await _client.GetAsync($"/api/clients/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var client = await response.Content.ReadFromJsonAsync<ClientDto>(JsonOptions);

        Assert.NotNull(client);

        return client;
    }

    public async Task DeleteClientAsync(long id)
    {
        var response = await _client.DeleteAsync($"/api/clients/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public async Task<List<ClientDto>> GetDeletedClientsAsync()
    {
        var response = await _client.GetAsync("/api/clients/deleted?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>(JsonOptions);

        Assert.NotNull(clients);

        return clients;
    }

    public static UpdateClientDto CreateUpdateDto()
    {
        return new UpdateClientDto
        {
            Email = "newEmail",
            PhoneNumber = "123",
            Comment = "temp"
        };
    }

    public async Task<List<ClientDto>> GetActiveClientsAsync()
    {
        var response = await _client.GetAsync("/api/clients?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>(JsonOptions);

        Assert.NotNull(clients);

        return clients;
    }

    public async Task<List<ClientDto>> GetActiveClientsWithParametersPageAsync(int page, int pageSize)
    {
        var response = await _client.GetAsync($"/api/clients?page={page}&pageSize={pageSize}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>(JsonOptions);

        Assert.NotNull(clients);

        return clients;
    }

    public async Task<List<long>> CreatingALimitedNumberOfClients(int countClients)
    {
        List<long> idClients = [];
        for (int i = 1; i < countClients + 1; i++)
        {
            string email = $"email{i}";
            string phoneNumber = $"+79{i}{i}{i}{i}{i}{i}{i}{i}{i}";
            string comment = $"Test comment {i}";
            var id = await CreateClientWithParameters(email, phoneNumber, comment);
            idClients.Add(id);
        }
        return idClients;
    }
}