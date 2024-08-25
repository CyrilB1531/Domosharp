using Domosharp.Business.Contracts.Models;

using MediatR;


namespace Domosharp.Business.Contracts.Commands.Devices;

public record CreateDeviceCommand : IRequest<Device?>
{
  public CreateDeviceCommand(string deviceId, string name)
  {
    DeviceId = deviceId;
    Name = name;
  }

  public int HardwareId { get; init; }

  public string DeviceId { get; init; }

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
