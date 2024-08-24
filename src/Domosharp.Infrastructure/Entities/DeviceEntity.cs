using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Domosharp.Infrastructure.Tests")]
namespace Domosharp.Infrastructure.Entities;

[Table("Device")]
internal record DeviceEntity
{
  public DeviceEntity()
  {
  }

  public DeviceEntity(int id)
  {
    Id = id;
  }

  public DeviceEntity(int id, string name, int hardwareId, string deviceId, int type) : this(id)
  {
    HardwareId = hardwareId;
    Name = name;
    DeviceId = deviceId;
    DeviceType = type;
  }

  [Key]
  [Column("Id")]
  public int Id { get; set; }

  [Column("HardwareId")]
  public int HardwareId { get; set; }

  [Column("DeviceId")]
  public string DeviceId { get; set; } = string.Empty;

  [Column("Name")]
  public string Name { get; set; } = string.Empty;

  [Column("Active")]
  public int Active { get; set; }

  [Column("Type")]
  public int DeviceType { get; set; }

  [Column("Favorite")]
  public int Favorite { get; set; }

  [Column("SignalLevel")]
  public int SignalLevel { get; set; }

  [Column("BatteryLevel")]
  public int BatteryLevel { get; set; }

  [Column("LastUpdate")]
  public DateTime LastUpdate { get; set; }

  [Column("Order")]
  public int Order { get; set; }

  [Column("Protected")]
  public int Protected { get; set; }

  [Column("SpecificParameters")]
  public string? SpecificParameters { get; set; }
}