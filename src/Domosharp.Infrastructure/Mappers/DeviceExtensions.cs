using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Infrastructure.Mappers
{
  internal static class DeviceExtensions
  {
    internal static DeviceEntity MapDeviceToEntity(this Device device)
    {
      return new DeviceEntity(device.Id, device.Name, device.HardwareId, device.DeviceId, (int)device.Type)
      {
        Active= device.Active ? 1 : 0,
        BatteryLevel = device.BatteryLevel,
        SpecificParameters = device.SpecificParameters,
        Favorite = device.Favorite ? 1 : 0,
        LastUpdate = device.LastUpdate,
        Order = device.Order,
        Protected = device.Protected ? 1 : 0,
        SignalLevel = device.SignalLevel,
      };
    }
  }
}
