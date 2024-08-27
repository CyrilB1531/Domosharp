
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domosharp.Infrastructure.Entities;

[Table("MqttHardware")]
public class MqttEntity
{
  public MqttEntity() { }

  public MqttEntity(int id)
  {
    Id = id;
  }

  [Key]
  [Column("ID")]
  public int Id { get; set; }

  [Column("Address")]
  public string Address { get; set; } = string.Empty;

  [Column("Port")]
  public int Port { get; set; }

  [Column("Username")]
  public string? Username { get; set; }

  [Column("Password")]
  public string? Password { get; set; }

  [Column("UseTLS")]
  public int UseTLS { get; set; }
}

