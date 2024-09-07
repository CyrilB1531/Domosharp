using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

using System.Text.Json;

namespace Domosharp.Infrastructure.Mappers
{
  internal static class TasmotaDeviceMapper
  {
    private static TasmotaDevice GetTasmotaDevice(Device device, TasmotaDiscoveryPayload payload) =>
      new(device.HardwareId, payload, device.Type)
      {
        BatteryLevel = device.BatteryLevel,
        Favorite = device.Favorite,
        Id = device.Id,
        LastUpdate = device.LastUpdate,
        Protected = device.Protected,
        SignalLevel = device.SignalLevel,
        Active = device.Active,
        SpecificParameters = device.SpecificParameters,
        Name = device.Name,
        DeviceId = device.DeviceId,
        Index = device.Index
      };

    internal static TasmotaDevice? MapToTasmotaDevice(this Device device)
    {
      if (string.IsNullOrWhiteSpace(device.SpecificParameters))
        return null;

      TasmotaDiscoveryPayload discoveryPayload;
      try
      {
        discoveryPayload = JsonSerializer.Deserialize<TasmotaDiscoveryPayload>(device.SpecificParameters, JsonExtensions.FullObjectOnDeserializing)!;
      }
      catch
      {
        return null;
      }

      return GetTasmotaDevice(device, discoveryPayload);
    }
  }
}
