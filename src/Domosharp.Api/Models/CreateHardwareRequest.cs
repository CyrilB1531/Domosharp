using Domosharp.Business.Contracts.Models;

using System.Text.Json.Serialization;

namespace Domosharp.Api.Models;

public record CreateHardwareRequest : UpdateHardwareRequest
{
  [JsonRequired]
  public HardwareType? Type { get; init; }
}
