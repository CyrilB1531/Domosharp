using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Mappers;

internal static class DeviceEntityExtensions
{
  internal static Device MapDeviceToDomain(this DeviceEntity device)
  {
    return new Device()
    {
      Id = device.Id,
      Active = device.Active != 0,
      BatteryLevel = device.BatteryLevel,
      DeviceId = device.DeviceId,
      Favorite = device.Favorite == 1,
      HardwareId = device.HardwareId,
      LastUpdate = device.LastUpdate,
      Name = device.Name,
      Protected = device.Protected == 1,
      SignalLevel = device.SignalLevel,
      Type = (DeviceType)device.DeviceType,
      SpecificParameters = device.SpecificParameters,
      Order = device.Order,
    };
  }
}
