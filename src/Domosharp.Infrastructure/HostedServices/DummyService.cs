using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using DotNetCore.CAP;

namespace Domosharp.Infrastructure.HostedServices;

internal class DummyService(ICapPublisher capPublisher, IDeviceRepository deviceRepository, IHardware hardware) : HardwareServiceBase(capPublisher, deviceRepository, hardware)
{

  public override Task ConnectAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public override Task DisconnectAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  protected override Task SendDataAsync(Device device, string command, int? value, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  protected override Task UpdateDataAsync(Device device, int? value, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
