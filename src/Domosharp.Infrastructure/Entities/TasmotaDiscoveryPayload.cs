using System.Text.Json.Serialization;

namespace Domosharp.Infrastructure.Entities;

internal record TasmotaDiscoveryPayload
{
  [JsonPropertyName("ip")]
  public string IP { get; set; } = string.Empty;
  [JsonPropertyName("dn")]
  public string DeviceName { get; set; } = string.Empty;
  [JsonPropertyName("fn")]
  public List<string?> FriendlyNames { get; set; } = [];

  [JsonPropertyName("hn")]
  public string HostName { get; set; } = string.Empty;
  [JsonPropertyName("mac")]
  public string FullMacAsDeviceId { get; set; } = string.Empty;
  [JsonPropertyName("md")]
  public string ModuleOrTemplateName { get; set; } = string.Empty;
  [JsonPropertyName("ty")]
  public int TuyaMCUFlag { get; set; }
  [JsonPropertyName("if")]
  public int IfanDevicesFlag { get; set; }
  [JsonPropertyName("ofln")]
  public string OfflinePayload { get; set; } = string.Empty;
  [JsonPropertyName("onln")]
  public string OnlinePayload { get; set; } = string.Empty;
  [JsonPropertyName("state")]
  public List<string?> States { get; set; } = [];
  [JsonPropertyName("sw")]
  public string SoftwareVersion { get; set; } = string.Empty;
  [JsonPropertyName("t")]
  public string Topic { get; set; } = string.Empty;
  [JsonPropertyName("ft")]
  public string FullTopic { get; set; } = string.Empty;
  [JsonPropertyName("tp")]
  public List<string?> TopicsForCommandStatAndTele { get; set; } = [];
  [JsonPropertyName("rl")]
  public List<RelayType> Relays { get; set; } = [];
  [JsonPropertyName("swc")]
  public List<int> SwitchModes { get; set; } = [];
  [JsonPropertyName("swn")]
  public List<string?> SwitchNames { get; set; } = [];
  [JsonPropertyName("btn")]
  public List<int> ButtonFlag { get; set; } = [];
  [JsonPropertyName("so")]
  public Dictionary<string, int> SetOptions { get; set; } = [];
  [JsonPropertyName("lk")]
  public int LightCTRGBlinked { get; set; }
  [JsonPropertyName("lt_st")]
  public int LightSubType { get; set; }
  [JsonPropertyName("bat")]
  public int Battery { get; set; }
  [JsonPropertyName("dslp")]
  public int DeepSleep { get; set; }
  [JsonPropertyName("sho")]
  public List<int> ShutterOptions { get; set; } = [];
  [JsonPropertyName("sht")]
  public List<int> ShutterTilt { get; set; } = [];
  [JsonPropertyName("ver")]
  public int DiscoveryVersion { get; set; }
}
