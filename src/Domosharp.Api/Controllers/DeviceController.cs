using Asp.Versioning;

using Domosharp.Api.Models;
using Domosharp.Api.Validators;
using Domosharp.Business.Contracts.Commands.Device;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Device;

using DotNetCore.CAP.Messages;

using MediatR;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Linq.Expressions;

namespace Domosharp.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
public class DeviceController(IMediator mediator) : ControllerBase
{
  [HttpPost]
  public async Task<ActionResult> AddAsync([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
  {
    var command = new CreateDeviceCommand(request.DeviceId!, request.Name!)
    {
      HardwareId = request.HardwareId!.Value,
      Active = request.Active!.Value,
      Type = request.Type!.Value,
      Favorite = request.Favorite!.Value,
      Order = request.Order!.Value,
      BatteryLevel = request.BatteryLevel!.Value,
      Protected = request.Protected!.Value,
      SignalLevel = request.SignalLevel!.Value,
      SpecificParameters = request.SpecificParameters
    };
    await mediator.Send(command, cancellationToken);
    return Ok();
  }

  [HttpGet("hardware/{id}")]
  public async Task<ActionResult<IEnumerable<DeviceResponse>>> GetListAsync(int id, CancellationToken cancellationToken)
  {
    var query = new GetAllDevicesQuery() { HardwareId = id };
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result.Select(a => new DeviceResponse(a)).ToList());
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
  {
    var query = new DeleteDeviceCommand() { Id = id };
    var result = await mediator.Send(query, cancellationToken);
    if (result)
      return Ok();
    else
      return BadRequest();
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult> UpdateAsync([FromBody] UpdateDeviceRequest request, int id, CancellationToken cancellationToken)
  {
    var command = new UpdateDeviceCommand(request.Name!)
    {
      Id = id,
      Active = request.Active!.Value,
      Type = request.Type!.Value,
      Favorite = request.Favorite!.Value,
      Order = request.Order!.Value,
      BatteryLevel = request.BatteryLevel!.Value,
      Protected = request.Protected!.Value,
      SignalLevel = request.SignalLevel!.Value,
      SpecificParameters = request.SpecificParameters
    }; 
    var result = await mediator.Send(command, cancellationToken);
    if (result)
      return Ok();
    else
      return BadRequest();
  }
}
