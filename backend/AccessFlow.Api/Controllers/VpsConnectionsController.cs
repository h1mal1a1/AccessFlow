using AccessFlow.Api.Contracts.Vps;
using AccessFlow.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AccessFlow.Api.Controllers;

[ApiController]
[Route("api/vps/connections")]
public class VpsConnectionsController(IVpsClient vpsClient) : ControllerBase
{
    [HttpGet("{name}")]
    public async Task<ActionResult<VpsConnectionInfo>> Get(string name, CancellationToken cancellationToken)
    {
        var connection = await vpsClient.GetConnectionAsync(name, cancellationToken);

        if (connection is null)
            return NotFound();

        return Ok(connection);
    }

    [HttpPost]
    public async Task<ActionResult<VpsConnectionInfo>> Create(CreateVpsConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await vpsClient.CreateConnectionAsync(request.Name, cancellationToken);

        return Ok(connection);
    }

    [HttpPut("{name}")]
    public async Task<ActionResult<VpsConnectionInfo>> Update(string name, UpdateVpsConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await vpsClient.RenameConnectionAsync(name, request.NewName, cancellationToken);

        return Ok(connection);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name, CancellationToken cancellationToken)
    {
        await vpsClient.DeleteConnectionAsync(name, cancellationToken);

        return NoContent();
    }
}