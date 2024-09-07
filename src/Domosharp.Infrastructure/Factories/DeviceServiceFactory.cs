using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.HostedServices;

namespace Domosharp.Infrastructure.Factories;

public class DeviceServiceFactory(IDeviceRepository deviceRepository) : IDeviceServiceFactory
{
  public Task<IDeviceService?> CreateDeviceServiceAsync(Device device, CancellationToken cancellationToken = default)
  {
    if (device is not TasmotaDevice tasmotaDevice)
      return Task.FromResult((IDeviceService?)null);
    return Task.FromResult((IDeviceService?)new TasmotaDeviceService(tasmotaDevice, deviceRepository));
  }
}
