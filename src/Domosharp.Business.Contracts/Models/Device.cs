namespace Domosharp.Business.Contracts.Models;

public record Device
{
  public Device()
  {
    Id = 0;
    DeviceId = string.Empty;
    Name = string.Empty;
  }

  public int Id { get; init; }

  public int HardwareId { get; init; }

  public string DeviceId { get; init; }

  public string Name { get; set; }

  public bool Active { get; set; }

  public DeviceType Type { get; set; }

  public bool Favorite { get; set; }

  public int SignalLevel { get; set; }

  public int BatteryLevel { get; set; }

  public int Order { get; set; }

  public DateTime LastUpdate { get; set; }

  public bool Protected { get; set; }

  public string? SpecificParameters { get; set; }

  public IHardware? Hardware { get; set; }

  public decimal? Value {  get; set; }

  public int? Index { get; set; }
}
