using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Factories;

public interface IHardwareInfrastructureFactory
{
  Task<IHardware?> CreateAsync(HardwareEntity entity, bool withPassword, CancellationToken cancellationToken = default);
}
