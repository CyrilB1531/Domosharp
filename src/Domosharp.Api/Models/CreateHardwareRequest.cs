using Domosharp.Business.Contracts.Models;

using System.Text.Json.Serialization;

namespace Domosharp.Api.Models;

public record CreateHardwareRequest
{
  [JsonRequired]
  public string? Name { get; init; }

  [JsonRequired]
  public bool? Enabled { get; init; }

  [JsonRequired]
  public HardwareType? Type { get; init; }

  [JsonRequired]
  public LogLevel? LogLevel { get; init; }

  [JsonRequired]
  public int? Order { get; init; }

  public string? Configuration { get; init; }
}
