using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Repositories;

internal interface IMqttEntityRepository
{
  Task<MqttEntity?> GetAsync(int id, CancellationToken cancellationToken = default);
  Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default);
}
