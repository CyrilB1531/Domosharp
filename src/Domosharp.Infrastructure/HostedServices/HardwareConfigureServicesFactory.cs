using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Factories;
using Domosharp.Infrastructure.Repositories;

using Microsoft.Extensions.DependencyInjection;

using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;

using MQTTnet.Implementations;
namespace Domosharp.Infrastructure.HostedServices;

public static class HardwareConfigureServicesFactory
{
  public static IServiceCollection AddHardwareServices(this IServiceCollection services)
  {
    services.AddTransient<IHardwareServiceFactory, HardwareServiceFactory>();
    services.AddTransient<IDeviceServiceFactory, DeviceServiceFactory>();
    services.AddTransient<IHardwareRepository, HardwareRepository>();
    services.AddTransient<MqttRepository>();
    services.AddTransient(a => (IMqttRepository)a.GetRequiredService<MqttRepository>());
    services.AddTransient(a => (IMqttEntityRepository)a.GetRequiredService<MqttRepository>());
    services.AddTransient<IMqttClient, MqttClient>();
    services.AddTransient<IMqttNetLogger, MqttNetNullLogger>();
    services.AddTransient<IMqttClientAdapterFactory, MqttClientAdapterFactory>();
    services.AddTransient<IManagedMqttClient, ManagedMqttClient>();
    services.AddTransient<HardwareFactory>();
    services.AddTransient(a => (IHardwareFactory)a.GetRequiredService<HardwareFactory>());
    services.AddTransient(a => (IHardwareInfrastructureFactory)a.GetRequiredService<HardwareFactory>());

    return services;
  }
}
