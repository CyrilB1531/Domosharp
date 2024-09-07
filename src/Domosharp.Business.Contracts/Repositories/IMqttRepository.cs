using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Repositories
{
  public interface IMqttRepository
  {
    Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default);
  }
}
