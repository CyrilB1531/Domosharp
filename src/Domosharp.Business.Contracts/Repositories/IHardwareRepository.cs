using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Repositories;

public interface IHardwareRepository
{
  int GetMaxId();
  Task<IHardware?> GetAsync(int hardwareId, bool withPassword, CancellationToken cancellationToken = default);
  Task<IEnumerable<IHardware>> GetListAsync(bool withPassword, CancellationToken cancellationToken = default);
  Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(int hardwareId, CancellationToken cancellationToken = default);
}
