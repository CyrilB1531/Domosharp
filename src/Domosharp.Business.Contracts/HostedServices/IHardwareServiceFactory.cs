using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.HostedServices;

public interface IHardwareServiceFactory
{
  IHardwareService CreateFromHardware(IHardware hardware);
}
