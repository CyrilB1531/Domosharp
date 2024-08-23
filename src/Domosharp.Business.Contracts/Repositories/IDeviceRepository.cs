using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Repositories;

public interface IDeviceRepository
{
  Task CreateAsync(Device device, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(int deviceId, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(Device device, CancellationToken cancellationToken = default);
  Task<Device?> GetAsync(int id, CancellationToken cancellationToken = default);
}
