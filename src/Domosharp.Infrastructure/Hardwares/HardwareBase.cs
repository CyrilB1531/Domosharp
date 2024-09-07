using Domosharp.Business.Contracts.Models;

using Microsoft.Extensions.Logging;

namespace Domosharp.Infrastructure.Hardwares;

internal abstract record HardwareBase : IHardware
{
  public EventHandler<DeviceEventArgs>? CreateDevice { get; set; }

  public EventHandler<DeviceEventArgs>? UpdateDevice { get; set; }

  public virtual void CopyTo(ref IHardware hardware)
  {
    hardware.Id = Id;
    hardware.Enabled = Enabled;
    hardware.LogLevel = LogLevel;
    hardware.Name = Name;
    hardware.Configuration = Configuration;
    hardware.Type = Type;
    hardware.Order = Order;
  }

  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public bool Enabled { get; set; }

  public HardwareType Type { get; set; }

  public LogLevel LogLevel { get; set; }

  public DateTime LastUpdate { get; set; }

  public string? Configuration { get; set; }

  public int Order { get; set; }
}
