using Domosharp.Business.Contracts.Models;

namespace Domosharp.Api.Models
{
  public record HardwareResponse
  {
    public HardwareResponse(IHardware hardware)
    {
      Id = hardware.Id;
      Name = hardware.Name;
      Enabled = hardware.Enabled;
      Type = hardware.Type;
      LogLevel = hardware.LogLevel;
      Configuration = hardware.Configuration;
      Order = hardware.Order;
      LastUpdate = hardware.LastUpdate;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public bool Enabled { get; init; }

    public HardwareType Type { get; init; }

    public LogLevel LogLevel { get; init; }

    public string? Configuration { get; init; }

    public int Order { get; init; }

    public DateTime LastUpdate { get; init; }
  }
}
