using Domosharp.Business.Contracts.Models;

using System.Text.Json.Serialization;

namespace Domosharp.Api.Models;

public record UpdateDeviceRequest
{
  [JsonRequired]
  public string? Name { get; init; }

  [JsonRequired]
  public bool? Active { get; init; }

  [JsonRequired]
  public DeviceType? Type { get; init; }

  [JsonRequired]
  public bool? Favorite { get; init; }

  [JsonRequired]
  public int? SignalLevel { get; init; }

  [JsonRequired]
  public int? BatteryLevel { get; init; }

  [JsonRequired]
  public int? Order { get; init; }

  [JsonRequired]
  public bool? Protected { get; init; }

  public string? SpecificParameters { get; init; }
}

