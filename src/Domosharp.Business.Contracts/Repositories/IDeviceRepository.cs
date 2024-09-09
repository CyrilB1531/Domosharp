using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Repositories;

public interface IDeviceRepository
{
  Task<Device?> CreateAsync(Device device, CancellationToken cancellationToken = default);
  Task<IEnumerable<Device>> GetListAsync(int hardwareId, CancellationToken cancellation = default);
  Task<IEnumerable<Device>> GetListAsync(bool onlyActives, bool onlyFavorites, CancellationToken cancellation = default);
  Task<bool> DeleteAsync(int deviceId, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(Device device, CancellationToken cancellationToken = default);
  Task<Device?> GetAsync(int id, CancellationToken cancellationToken = default);
}
