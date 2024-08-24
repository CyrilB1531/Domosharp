using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Models;

public interface IHardware
{
  public int Id { get; }
  string Name { get; set; }

  bool Enabled { get; set; }

  HardwareType Type { get; set; }

  LogLevel LogLevel { get; set; }

  string? Configuration { get; set; }

  DateTime LastUpdate { get; }

  int Order { get; set; }
}
