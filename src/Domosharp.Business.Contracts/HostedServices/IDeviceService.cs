using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.HostedServices;

public interface IDeviceService
{
  public Device Device { get; }
}
