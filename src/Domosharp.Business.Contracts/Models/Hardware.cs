using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Models
{
  public class Hardware : IHardware
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public HardwareType Type { get; set; }
    public LogLevel LogLevel { get; set; }
    public string? Configuration { get; set; }
    public int Order { get; set; }
  }
}
