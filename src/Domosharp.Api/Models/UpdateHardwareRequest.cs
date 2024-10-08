﻿using System.Text.Json.Serialization;

namespace Domosharp.Api.Models;

public record UpdateHardwareRequest
{
  [JsonRequired]
  public string? Name { get; init; }

  [JsonRequired]
  public bool? Enabled { get; init; }

  [JsonRequired]
  public LogLevel? LogLevel { get; init; }

  [JsonRequired]
  public int? Order { get; init; }

  public string? Configuration { get; init; }

}
