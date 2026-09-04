using AccessFlow.Api.Contracts.Clients;
using AccessFlow.Application.Clients;
using AccessFlow.Application.Clients.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AccessFlow.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;

    [HttpGet("{id:long}")]
    public async Task<ClientDto> GetClientById(long id, CancellationToken cancellationToken)
        => await _clientService.GetClientAsync(id, cancellationToken);

    [HttpGet]
    public async Task<List<ClientDto>> GetClients(CancellationToken cancellationToken)
        => await _clientService.GetClientsAsync(cancellationToken);

    [HttpPost]
    public async Task CreateClient(CreateClientRequest request, CancellationToken cancellationToken)
    {
        CreateClientDto clientDto = new()
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Comment = request.Comment
        };
        await _clientService.CreateClientAsync(clientDto, cancellationToken);
    }
    [HttpDelete("{id:long}")]
    public async Task DeleteClient(long id, CancellationToken cancellationToken)
    {
        await _clientService.DeleteClientAsync(id, cancellationToken);
    }
    [HttpPut("{id:long}")]
    public async Task UpdateClient(long id, UpdateClientRequest updateClientRequest, CancellationToken cancellationToken)
    {
        UpdateClientDto updateClientDto = new()
        {
            Email = updateClientRequest.Email,
            PhoneNumber = updateClientRequest.PhoneNumber,
            Comment = updateClientRequest.Comment,
        };
        await _clientService.UpdateClientAsync(id, updateClientDto, cancellationToken);
    }

    [HttpGet("deleted")]
    public async Task<List<ClientDto>> GetDeletedClientsAsync(CancellationToken cancellationToken) =>
        await _clientService.GetDeletedClientsAsync(cancellationToken);
}