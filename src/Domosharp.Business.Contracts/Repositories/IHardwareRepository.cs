using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Repositories
{
  public interface IHardwareRepository
  {
    Task<IHardware?> GetAsync(int hardwareId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IHardware>> GetListAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int hardwareId, CancellationToken cancellationToken = default);
  }
}
