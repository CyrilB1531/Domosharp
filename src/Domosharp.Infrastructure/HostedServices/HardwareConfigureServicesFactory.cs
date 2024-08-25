using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Repositories;

using Microsoft.Extensions.DependencyInjection;
namespace Domosharp.Infrastructure.HostedServices;

public static class HardwareConfigureServicesFactory
{
  public static IServiceCollection AddHardwareServices(this IServiceCollection services)
  {
    services.AddTransient<IHardwareServiceFactory, HardwareServiceFactory>();
    services.AddTransient<IHardwareRepository, HardwareRepository>();

    return services;
  }
}
