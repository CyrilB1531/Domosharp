using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Domosharp.Infrastructure.Tests")]
namespace Domosharp.Infrastructure.Entities;

[Table("Device")]
internal class DeviceEntity
{
  public DeviceEntity()
  {
    Id = 0;
    HardwareId = 0;
    DeviceId = string.Empty;
    Name = string.Empty;
    Active = 0;
    Order = 0;
    DeviceType = 0;
    Favorite = 0;
    SignalLevel = 0;
    BatteryLevel = 0;
    LastUpdate = DateTime.Now;
    Protected = 0;
    SpecificParameters = string.Empty;
  }

  public DeviceEntity(int id) : this()
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
  public string DeviceId { get; set; }

  [Column("Name")]
  public string Name { get; set; }

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