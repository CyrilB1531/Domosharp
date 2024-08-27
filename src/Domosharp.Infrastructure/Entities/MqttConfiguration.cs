namespace Domosharp.Infrastructure.Entities;

public record MqttConfiguration
{
  public string Address { get; init; } = string.Empty;

  public int Port { get; init; }

  public bool UseTLS { get; init; }

  public string? UserName { get; init; }

  public string? Password { get; init; }

  public string[] SubscriptionsIn {  get; init; } = [];

  public string[] SubscriptionsOut { get; init; } = [];
}
