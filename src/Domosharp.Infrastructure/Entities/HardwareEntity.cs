using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domosharp.Infrastructure.Entities;

[Table("Hardware")]
public record HardwareEntity
{
  public HardwareEntity()
  {
  }

  public HardwareEntity(int id)
  {
    Id = id;
  }

  [Key]
  [Column("Id")]
  public int Id { get; set; }

  [Column("Name")]
  public string Name { get; set; } = string.Empty;

  [Column("Enabled")]
  public int Enabled { get; set; }

  [Column("Type")]
  public int Type { get; set; }

  [Column("LogLevel")]
  public int LogLevel { get; set; }

  [Column("Order")]
  public int Order { get; set; }

  [Column("Configuration")]
  public string? Configuration { get; set; }

  [Column("LastUpdate")]
  public DateTime LastUpdate { get; set; }
}
