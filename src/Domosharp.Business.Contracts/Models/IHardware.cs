using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Models;

public interface IHardware
{
  EventHandler<DeviceEventArgs>? CreateDevice { get; set; }
  EventHandler<DeviceEventArgs>? UpdateDevice { get; set; }

  void CopyTo(ref IHardware hardware);

  int Id { get; set; }

  string Name { get; set; }

  bool Enabled { get; set; }

  HardwareType Type { get; set; }

  LogLevel LogLevel { get; set; }

  string? Configuration { get; set; }

  DateTime LastUpdate { get; }

  int Order { get; set; }
}
