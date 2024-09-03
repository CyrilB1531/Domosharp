using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

using Newtonsoft.Json;

namespace Domosharp.Infrastructure.Mappers
{
  internal static class TasmotaDeviceMapper
  {
    private static TasmotaDevice GetTasmotaDevice(Device device, TasmotaDiscoveryPayload payload) =>
      new(device.Hardware, payload)
      {
        BatteryLevel = device.BatteryLevel,
        Favorite = device.Favorite,
        Id = device.Id,
        LastUpdate = device.LastUpdate,
        Protected = device.Protected,
        SignalLevel = device.SignalLevel,
        Type = device.Type,
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
        discoveryPayload = JsonConvert.DeserializeObject<TasmotaDiscoveryPayload>(device.SpecificParameters, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error })!;
      }
      catch
      {
        return null;
      }

      return GetTasmotaDevice(device, discoveryPayload);
    }
  }
}
