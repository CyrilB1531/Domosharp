﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domosharp.Infrastructure.Entities;

[Table("Hardware")]
public class HardwareEntity
{
  public HardwareEntity()
  {
    Name = string.Empty;
  }

  public HardwareEntity(int id) : this()
  {
    Id = id;
  }

  [Key]
  [Column("Id")]
  public int Id { get; set; }

  [Column("Name")]
  public string Name { get; set; }

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
}
