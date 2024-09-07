using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Factories;

public interface IDeviceServiceFactory
{
  public Task<IDeviceService?> CreateDeviceServiceAsync(Device device, CancellationToken cancellationToken = default);
}
