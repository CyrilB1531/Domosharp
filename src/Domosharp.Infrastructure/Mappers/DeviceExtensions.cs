using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Mappers;

internal static class DeviceExtensions
{
  internal static DeviceEntity MapToEntity(this Device device, int id) => new(id, device.Name, device.HardwareId, device.DeviceId, (int)device.Type)
    {
      Active = device.Active ? 1 : 0,
      BatteryLevel = device.BatteryLevel,
      SpecificParameters = device.SpecificParameters,
      Favorite = device.Favorite ? 1 : 0,
      LastUpdate = device.LastUpdate,
      Order = device.Order,
      Protected = device.Protected ? 1 : 0,
      SignalLevel = device.SignalLevel,
      Value = device.Value,
      Index = device.Index
    };
}
