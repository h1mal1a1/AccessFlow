using AccessFlow.Api.Contracts.Clients;
using AccessFlow.Api.Options;
using AccessFlow.Application.Clients;
using AccessFlow.Application.Clients.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace AccessFlow.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService, IOptions<PaginationOptions> paginationOptions)
    : ControllerBase
{
    private readonly IClientService _clientService = clientService;
    private readonly PaginationOptions _paginationOptions = paginationOptions.Value;

    [HttpGet("{id:long}")]
    public async Task<ClientDto> GetClientById(long id, CancellationToken cancellationToken)
        => await _clientService.GetClientAsync(id, cancellationToken);

    [HttpGet]
    public async Task<List<ClientDto>> GetClients(int page = 1, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var actualPageSize = pageSize ?? _paginationOptions.DefaultPageSize;
        if (page < 1)
            page = 1;

        if (actualPageSize > _paginationOptions.MaxPageSize)
            actualPageSize = _paginationOptions.MaxPageSize;
        if (actualPageSize < 1)
            actualPageSize = _paginationOptions.DefaultPageSize;

        return await _clientService.GetClientsAsync(page, actualPageSize, cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<long>> CreateClient(CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        CreateClientDto clientDto = new()
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Comment = request.Comment
        };
        var id = await _clientService.CreateClientAsync(clientDto, cancellationToken);
        return CreatedAtAction(nameof(GetClientById), new { id }, id);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteClient(long id, CancellationToken cancellationToken)
    {
        await _clientService.DeleteClientAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateClient(long id, UpdateClientRequest updateClientRequest,
        CancellationToken cancellationToken)
    {
        UpdateClientDto updateClientDto = new()
        {
            Email = updateClientRequest.Email,
            PhoneNumber = updateClientRequest.PhoneNumber,
            Comment = updateClientRequest.Comment,
        };
        await _clientService.UpdateClientAsync(id, updateClientDto, cancellationToken);
        return NoContent();
    }

    [HttpGet("deleted")]
    public async Task<List<ClientDto>> GetDeletedClients(int page = 1, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var actualPageSize = pageSize ?? _paginationOptions.DefaultPageSize;
        if (page < 1)
            page = 1;

        if (actualPageSize > _paginationOptions.MaxPageSize)
            actualPageSize = _paginationOptions.MaxPageSize;
        if (actualPageSize < 1)
            actualPageSize = _paginationOptions.DefaultPageSize;
        return await _clientService.GetDeletedClientsAsync(page, actualPageSize, cancellationToken);
    }

}