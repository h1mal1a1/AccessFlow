using AccessFlow.Api.Options;
using AccessFlow.Api.Contracts.Connections;
using AccessFlow.Application.Connections;
using AccessFlow.Application.Connections.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AccessFlow.Api.Controllers;

[ApiController]
[Route("api/connections")]
public class ConnectionsController(IConnectionService connectionService, IOptions<PaginationOptions> paginationOptions)
    : ControllerBase
{
    private readonly IConnectionService _connectionService = connectionService;
    private readonly PaginationOptions _paginationOptions = paginationOptions.Value;

    [HttpGet("{id:long}")]
    public async Task<ConnectionDto> GetConnectionById(long id, CancellationToken cancellationToken)
        => await _connectionService.GetConnectionAsync(id, cancellationToken);
    [HttpGet]
    public async Task<List<ConnectionDto>> GetConnections(int page = 1, int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var actualPageSize = pageSize ?? _paginationOptions.DefaultPageSize;
        if (page < 1)
            page = 1;

        if (actualPageSize > _paginationOptions.MaxPageSize)
            actualPageSize = _paginationOptions.MaxPageSize;
        if (actualPageSize < 1)
            actualPageSize = _paginationOptions.DefaultPageSize;

        return await _connectionService.GetConnectionsAsync(page, actualPageSize, cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<long>> CreateConnection(CreateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        CreateConnectionDto connectionDto = new()
        {
            IdClient = request.IdClient,
            IdExternal = request.IdExternal,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            SubUrl = request.SubUrl
        };
        var id = await _connectionService.CreateConnectionAsync(connectionDto, cancellationToken);
        return CreatedAtAction(nameof(GetConnectionById), new { id }, id);
    }
    [HttpDelete("{id:long}")]
    public async Task DeleteConnection(long id, CancellationToken cancellationToken)
    {
        await _connectionService.DeleteConnectionAsync(id, cancellationToken);
    }
    [HttpPut("{id:long}")]
    public async Task UpdateConnection(long id, UpdateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        UpdateConnectionDto updateConnectionDto = new()
        {
            IdExternal = request.IdExternal,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            SubUrl = request.SubUrl
        };
        await _connectionService.UpdateConnectionAsync(id, updateConnectionDto, cancellationToken);
    }

    [HttpGet("deleted")]
    public async Task<List<ConnectionDto>> GetDeletedConnectionsAsync(CancellationToken cancellationToken) =>
        await _connectionService.GetDeletedConnectionsAsync(cancellationToken);
}