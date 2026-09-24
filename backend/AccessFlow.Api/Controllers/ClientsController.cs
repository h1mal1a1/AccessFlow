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
    public async Task<List<ClientDto>> GetClients(int page = 1, int? pageSize = null, CancellationToken ct = default)
    {
        var (pageNumber, actualPageSize) = PaginationHelper.Normalize(page, pageSize, _paginationOptions);
        return await _clientService.GetClientsAsync(pageNumber, actualPageSize, ct);
    }

    [HttpPost]
    public async Task<ActionResult<long>> CreateClient(CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var clientDto = new CreateClientDto(request.Email, request.PhoneNumber, request.Comment);
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
    public async Task<IActionResult> UpdateClient(long id, UpdateClientRequest updateCliReq, CancellationToken ct)
    {
        var updateClientDto = new UpdateClientDto(updateCliReq.Email, updateCliReq.PhoneNumber, updateCliReq.Comment);
        await _clientService.UpdateClientAsync(id, updateClientDto, ct);
        return NoContent();
    }

    [HttpGet("deleted")]
    public async Task<List<ClientDto>> GetDeletedClients(int page = 1, int? pageSize = null,
        CancellationToken ct = default)
    {
        var (pageNumber, actualPageSize) = PaginationHelper.Normalize(page, pageSize, _paginationOptions);
        return await _clientService.GetDeletedClientsAsync(pageNumber, actualPageSize, ct);
    }

}