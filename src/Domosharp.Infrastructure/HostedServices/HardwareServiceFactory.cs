using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using DotNetCore.CAP;

namespace Domosharp.Infrastructure.HostedServices;

public class HardwareServiceFactory(
  ICapPublisher capPublisher,
    IDeviceRepository deviceRepository) : IHardwareServiceFactory
{
  public IHardwareService CreateFromHardware(IHardware hardware)
  {
    return hardware.Type switch
    {
      HardwareType.Dummy => new DummyService(capPublisher, deviceRepository, hardware),
      _ => throw new NotImplementedException(),
    };
  }
}
