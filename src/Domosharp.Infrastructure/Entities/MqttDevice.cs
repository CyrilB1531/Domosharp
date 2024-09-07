using Domosharp.Business.Contracts.Models;

namespace Domosharp.Infrastructure.Entities;

public record MqttDevice : Device
{
  public MqttDevice(int hardwareId, DeviceType? deviceType = null, int? index = null)
  {
    HardwareId = hardwareId;
    if (deviceType is not null)
      Type = deviceType.Value;
    Index = index;
  }
}
