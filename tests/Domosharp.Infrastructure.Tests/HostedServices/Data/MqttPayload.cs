using Bogus;

using Domosharp.Infrastructure.Entities;

using System.Text.Json;

namespace Domosharp.Infrastructure.Tests.HostedServices.Data;

internal static class MqttPayload
{
  public static TasmotaDiscoveryPayload Clone(this TasmotaDiscoveryPayload payload)
  {
    return new()
    {
      Battery = payload.Battery,
      ButtonFlag = payload.ButtonFlag,
      DeepSleep = payload.DeepSleep,
      DeviceName = payload.DeviceName,
      DiscoveryVersion = payload.DiscoveryVersion,
      FriendlyNames = [.. payload.FriendlyNames],
      FullMacAsDeviceId = payload.FullMacAsDeviceId,
      FullTopic = payload.FullTopic,
      HostName = payload.HostName,
      IfanDevicesFlag = payload.IfanDevicesFlag,
      IP = payload.IP,
      LightCTRGBlinked = payload.LightCTRGBlinked,
      LightSubType = payload.LightSubType,
      ModuleOrTemplateName = payload.ModuleOrTemplateName,
      OfflinePayload = payload.OfflinePayload,
      OnlinePayload = payload.OnlinePayload,
      Relays = [.. payload.Relays],
      SetOptions = payload.SetOptions,
      ShutterOptions = [.. payload.ShutterOptions],
      ShutterTilt = [.. payload.ShutterTilt],
      SoftwareVersion = payload.SoftwareVersion,
      States = [.. payload.States],
      SwitchModes = [.. payload.SwitchModes],
      SwitchNames = [.. payload.SwitchNames],
      Topic = payload.Topic,
      TopicsForCommandStatAndTele = [.. payload.TopicsForCommandStatAndTele],
      TuyaMCUFlag = payload.TuyaMCUFlag
    };
  }
  public static TasmotaDiscoveryPayload GetDevicesPayload(int deviceCount, RelayType type, string? mac = null, string? deviceName = null) => new Faker<TasmotaDiscoveryPayload>()
      .Rules((faker, device) =>
      {
        var deviceTypes = new List<RelayType>() { RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None,
        RelayType.None};
        var index = 0;
        for (var i = 0; i < deviceCount; i++)
        {
          deviceTypes[index++] = type;
          if (type == RelayType.Shutter)
            deviceTypes[index++] = type;
        }
        if (string.IsNullOrWhiteSpace(mac))
          mac = faker.Internet.Mac().Replace(":", string.Empty);
        device.IP = faker.Internet.Ip();
        device.DeviceName = deviceName ?? faker.Name.LastName();
        device.FriendlyNames = [faker.Name.FirstName(), null, null, null];
        device.HostName = $"tasmota-{mac[7..]}";
        device.FullMacAsDeviceId = mac;
        device.ModuleOrTemplateName = faker.Name.LastName();
        device.TuyaMCUFlag = false;
        device.IfanDevicesFlag = false;
        device.OfflinePayload = "Offline";
        device.OnlinePayload = "Online";
        device.States = ["OFF", "ON", "TOGGLE", "HOLD"];
        device.SoftwareVersion = faker.System.Version().ToString();
        device.Topic = device.HostName;
        device.FullTopic = "%prefix%/%topic%/";
        device.TopicsForCommandStatAndTele = ["cmnd", "stat", "tele"];
        device.Relays = deviceTypes;
        device.SwitchModes = [ 
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None,
          SwitchMode.None, SwitchMode.None, SwitchMode.None, SwitchMode.None];
        device.SwitchNames = [ null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null];
        device.ButtonFlag = [ false, false, false, false,
          false, false, false, false,
          false, false, false, false,
          false, false, false, false,
          false, false, false, false,
          false, false, false, false,
          false, false, false, false,
          false, false, false, false];
        device.SetOptions = new Dictionary<string, int>() { { "4", 0 }, { "11", 0}, { "13", 0}, { "17", 1},
  { "20", 0}, { "30", 0}, { "68", 0}, { "73", 0}, { "82", 0}, { "114", 0}, { "117", 0}};
        device.LightCTRGBlinked = 0;
        device.LightSubType = 0;
        device.Battery = 0;
        device.DeepSleep = 0;
        device.ShutterOptions = [];
        device.ShutterTilt = [];
        device.DiscoveryVersion = 1;
      }).Generate();

  public record WifiSatusPayload
  {
    public int AP { get; } = 1;
    public string SSId { get; } = "SSID";
    public string BSSId { get; } = "F0:A7:31:1E:5C:C6";
    public int Channel { get; } = 1;
    public string Mode { get; } = "HT20";
    public int RSSI { get; } = 50;
    public int Signal { get; } = -75;
    public int LinkCount { get; } = 1;
    public TimeSpan Downtime { get; } = TimeSpan.FromSeconds(4);
  }

  public record TeleSatus1Payload
  {
    public DateTime Time { get; } = DateTime.Now;
    public TimeSpan Uptime { get; } = TimeSpan.FromSeconds((9 * 60 + 10) * 60 + 56);
    public double UptimeSec { get; } = TimeSpan.FromSeconds((9 * 60 + 10) * 60 + 56).TotalSeconds;
    public int Heap { get; } = 162;
    public string SleepMode { get; } = "Dynamic";
    public int Sleep { get; } = 20;
    public int LoadAvg { get; } = 49;
    public int MqttCount { get; } = 5;
    public string POWER { get; init; } = "OFF";
    public WifiSatusPayload Wifi { get; } = new WifiSatusPayload();
  }

  public record TeleSatus2Payload
  {
    public DateTime Time { get; } = DateTime.Now;
    public TimeSpan Uptime { get; } = TimeSpan.FromSeconds((9 * 60 + 10) * 60 + 56);
    public double UptimeSec { get; } = TimeSpan.FromSeconds((9 * 60 + 10) * 60 + 56).TotalSeconds;
    public int Heap { get; } = 162;
    public string SleepMode { get; } = "Dynamic";
    public int Sleep { get; } = 20;
    public int LoadAvg { get; } = 49;
    public int MqttCount { get; } = 5;
    public string POWER1 { get; init; } = "OFF";
    public string POWER2 { get; init; } = "OFF";
    public WifiSatusPayload Wifi { get; } = new WifiSatusPayload();
  }

  public record Esp32Payload
  {
    public decimal Temperature { get; set; }
  }

  public record SensorPayload
  {
    public DateTime Time { get; set; }
    public string Switch1 { get; set; } = "OFF";
    public string Switch2 { get; set; } = "OFF";
    public Esp32Payload? ESP32 { get; set; }
    public TasmotaShutterPayload Shutter1 { get; set; } = new TasmotaShutterPayload();
  }

  public static string GetLightState(string value)
  {
    return JsonSerializer.Serialize(new TeleSatus1Payload() { POWER = value }, JsonExtensions.FullObjectOnDeserializing);
  }

  public static string GetTwoLightsState(string value1, string value2)
  {
    return JsonSerializer.Serialize(new TeleSatus2Payload() { POWER1 = value1, POWER2 = value2 }, JsonExtensions.FullObjectOnDeserializing);
  }

  public static string GetSensor(int position, int target, bool useEsp32Node = true, bool useTemperature = true)
  {
    var sensor = new SensorPayload();
    sensor.Shutter1.Position = position;
    sensor.Shutter1.Target = target;
    if (useEsp32Node)
    {
      sensor.ESP32 = new()
      {
        Temperature = 49
      };
      if (!useTemperature)
      {
        var result = JsonSerializer.Serialize(sensor, JsonExtensions.FullObjectOnDeserializing).Replace("\"Temperature\":49", string.Empty);
        return result;
      }
    }
    return JsonSerializer.Serialize(sensor, JsonExtensions.FullObjectOnDeserializing);
  }

  public static string GetResultState(int shutterIndex, int position, int target)
  {
    var shutter1 = new TasmotaShutterPayload()
    {
      Direction = 0,
      Position = position,
      Target = target,
      Tilt = 0
    };
    var result = "{\"Shutter" + shutterIndex + "\":" + JsonSerializer.Serialize(shutter1, JsonExtensions.FullObjectOnDeserializing) + "}";
    return result;
  }

}
