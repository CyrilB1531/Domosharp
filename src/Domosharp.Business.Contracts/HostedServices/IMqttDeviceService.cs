using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.HostedServices;

public interface IMqttDeviceService : IDeviceService
{
  public IEnumerable<string> GetSubscriptions();

  public Task<bool> HandleAsync(string command, string payload, CancellationToken cancellationToken = default);
}
