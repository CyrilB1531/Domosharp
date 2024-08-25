using Domosharp.Business.Contracts.Models;

using System.Text.Json.Serialization;

namespace Domosharp.Api.Models;

public record CreateDeviceRequest : UpdateDeviceRequest
{
  [JsonRequired]
  public int? HardwareId { get; init; }

  [JsonRequired]
  public string? DeviceId { get; init; }
}
