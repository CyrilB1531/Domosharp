using Domosharp.Business.Contracts.Models;

using MediatR;

namespace Domosharp.Business.Contracts.Commands.Devices;

public record UpdateDeviceCommand : IRequest<bool>
{
  public UpdateDeviceCommand(string name)
  {
    Name = name;
  }

  public int Id { get; init; }

  public string Name { get; init; }

  public bool Active { get; init; }

  public DeviceType Type { get; init; }

  public bool Favorite { get; init; }

  public int SignalLevel { get; init; }

  public int BatteryLevel { get; init; }

  public int Order { get; init; }

  public bool Protected { get; init; }

  public string? SpecificParameters { get; init; }
}
