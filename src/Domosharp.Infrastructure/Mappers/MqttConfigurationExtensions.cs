using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Mappers;

internal static class MqttConfigurationExtensions
{
  internal static MqttConfiguration Clone(this MqttConfiguration configuration, bool withPassword)
  {
    return new()
    {
      Address = configuration.Address,
      Password = withPassword ? configuration.Password : "****",
      Port = configuration.Port,
      SubscriptionsIn = configuration.SubscriptionsIn,
      SubscriptionsOut = configuration.SubscriptionsOut,
      UserName = configuration.UserName,
      UseTLS = configuration.UseTLS
    };
  }
}
