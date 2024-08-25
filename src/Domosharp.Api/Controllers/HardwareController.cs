using Asp.Versioning;

using Domosharp.Api.Models;
using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Queries.Hardware;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Domosharp.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
public class HardwareController(IMediator mediator) : ControllerBase
{
  [HttpPost]
  public async Task<ActionResult> AddAsync([FromBody] CreateHardwareRequest createHardwareRequest, CancellationToken cancellationToken)
  {
    var command = new CreateHardwareCommand
    {
      Name = createHardwareRequest.Name!,
      Enabled = createHardwareRequest.Enabled!.Value,
      Type = createHardwareRequest.Type!.Value,
      LogLevel = createHardwareRequest.LogLevel!.Value,
      Order = createHardwareRequest.Order!.Value,
      Configuration = createHardwareRequest.Configuration
    };
    await mediator.Send(command, cancellationToken);
    return Ok();
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<HardwareResponse>>> GetListAsync(CancellationToken cancellationToken)
  {
    var query = new GetAllHardwaresQuery();
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result.Select(a => new HardwareResponse(a)).ToList());
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult> UpadateAsync([FromBody] UpdateHardwareRequest hardware, int id, CancellationToken cancellationToken)
  {
    var command = new UpdateHardwareCommand
    {
      Id = id,
      Name = hardware.Name!,
      Enabled = hardware.Enabled!.Value,
      Type = hardware.Type!.Value,
      LogLevel = hardware.LogLevel!.Value,
      Order = hardware.Order!.Value,
      Configuration = hardware.Configuration
    };
    var result = await mediator.Send(command, cancellationToken);
    if (result)
      return Ok();
    else
      return BadRequest();
  }

  [HttpDelete("{hardwareId}")]
  public async Task<ActionResult> DeleteAsync(int hardwareId, CancellationToken cancellationToken)
  {
    var command = new DeleteHardwareCommand
    {
      Id = hardwareId
    };

    var result = await mediator.Send(command, cancellationToken);
    if (result)
      return Ok();
    else
      return BadRequest();
  }
}
