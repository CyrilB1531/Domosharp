using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.HostedServices;

public interface IHardwareWorker
{
  Task DoWorkAsync(IEnumerable<IHardware?> hardwares, CancellationToken cancellationToken = default);
  Task StopAsync(CancellationToken cancellationToken = default);

  Task UpdateValueAsync(Device device, int? value, CancellationToken cancellationToken = default);
  Task SendValueAsync(Device device, string command, int? value, CancellationToken cancellationToken = default);
}
