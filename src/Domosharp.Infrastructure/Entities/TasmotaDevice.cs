using Domosharp.Business.Contracts.Models;

using System.Text.Json;

namespace Domosharp.Infrastructure.Entities;

internal record TasmotaDevice : Device
{
  public TasmotaDevice(IHardware? hardware, TasmotaDiscoveryPayload discoveryPayload, int? index = null)
  {
    if (discoveryPayload.TopicsForCommandStatAndTele.Count != 3)
      throw new ArgumentException("TopicsForCommandStatAndTele has not exactly 3 items.", nameof(discoveryPayload));
    const string prefix = "%prefix%";
    DeviceId = discoveryPayload.FullMacAsDeviceId;
    Hardware = hardware;
    HardwareId = hardware?.Id ?? 0;
    var topic = discoveryPayload.FullTopic.Replace("%topic%", discoveryPayload.Topic);
    CommandTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[0]);
    StateTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[1]);
    TelemetryTopic = topic.Replace(prefix, discoveryPayload.TopicsForCommandStatAndTele[2]);
    if (discoveryPayload.Relays[0] == RelayType.Simple)
      Type = DeviceType.LightSwitch;
    else if (discoveryPayload.Relays[0] == RelayType.Light)
      Type = DeviceType.LightSwitch;
    else if (discoveryPayload.Relays[0] == RelayType.Shutter)
      Type = DeviceType.Blinds;
    Index = index;
    if (index is not null)
      Name = $"{discoveryPayload.DeviceName}_{index}";
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
