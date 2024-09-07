using Domosharp.Business.Contracts.Models;

using System.Text.Json;

namespace Domosharp.Infrastructure.Entities;

internal record TasmotaDevice : MqttDevice
{
  private static string GetDeviceName(TasmotaDiscoveryPayload payload, DeviceType deviceType, int? index = null)
  {
    var internalIndex = index ?? 1;
    var name = string.Empty;
    if (deviceType == DeviceType.Sensor)
      name = "Temperature_";
    if (!string.IsNullOrWhiteSpace(payload.FriendlyNames[internalIndex]))
      return $"{name}{payload.FriendlyNames[internalIndex]!}";
    if (index is not null)
      return $"{name}{payload.DeviceName}_{index}";
    return $"{name}{payload.DeviceName}";
  }

  public TasmotaDevice(int hardwareId, TasmotaDiscoveryPayload discoveryPayload, DeviceType? deviceType = null, int? index = null): base(hardwareId, deviceType, index)
  {
    if (discoveryPayload.TopicsForCommandStatAndTele.Count != 3)
      throw new ArgumentException("TopicsForCommandStatAndTele has not exactly 3 items.", nameof(discoveryPayload));
    const string prefix = "%prefix%";
    DeviceId = discoveryPayload.FullMacAsDeviceId;
    var topic = discoveryPayload.FullTopic.Replace("%topic%", discoveryPayload.Topic);
    CommandTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[0]);
    StateTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[1]);
    TelemetryTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[2]);
    if (deviceType is null)
    {
      if (discoveryPayload.Relays[0] == RelayType.Simple)
        Type = DeviceType.LightSwitch;
      else if (discoveryPayload.Relays[0] == RelayType.Light)
        Type = DeviceType.LightSwitch;
      else if (discoveryPayload.Relays[0] == RelayType.Shutter)
        Type = DeviceType.Blinds;
    }
    Name = GetDeviceName(discoveryPayload, Type, index);
    States = discoveryPayload.States;
    MacAddress = discoveryPayload.FullMacAsDeviceId;

    SpecificParameters = JsonSerializer.Serialize(discoveryPayload, JsonExtensions.FullObjectOnDeserializing);
  }

  public string TelemetryTopic { get; }

  public string StateTopic { get; }

  public string CommandTopic { get; }

  public string MacAddress { get; }

  public List<string?> States { get; }
}
