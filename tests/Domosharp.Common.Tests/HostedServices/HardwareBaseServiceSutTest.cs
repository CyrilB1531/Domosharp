using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.HostedServices;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging.Abstractions;

using System.Diagnostics.CodeAnalysis;

namespace Domosharp.Common.Tests.HostedServices;

[ExcludeFromCodeCoverage]
public class HardwareBaseServiceSutTest(ICapPublisher capPublisher, IDeviceRepository deviceRepository, IDeviceServiceFactory deviceServiceFactory, IHardware hardware) : HardwareServiceBase(capPublisher, deviceRepository, hardware, deviceServiceFactory, NullLogger.Instance)
{
  public override Task ConnectAsync(CancellationToken cancellationToken)
  {
    ConnectCount++;
    return Task.CompletedTask;
  }

  public override Task DisconnectAsync(CancellationToken cancellationToken)
  {
    DisconnectCount++;
    return Task.CompletedTask;
  }

  public int ConnectCount { get; private set; }
  public int DisconnectCount { get; private set; }

  public int EnqueueMessageCount { get; private set; }
  public int DequeueMessageCount { get; private set; }
  public int SendValueCount { get; private set; }
  public int UpdateValueCount { get; private set; }
  public IMessage? DequeueMessageValue { get; set; }

  public override void EnqueueMessage(IMessage message)
  {
    EnqueueMessageCount++;
    base.EnqueueMessage(message);
  }
  public override IMessage? DequeueMessage()
  {
    DequeueMessageCount++;
    var message = DequeueMessageValue;
    if (DequeueMessageValue is not null)
      DequeueMessageValue = null;
    return message ?? base.DequeueMessage();
  }
  protected override Task SendDataAsync(Device device, string command, int? value, CancellationToken cancellationToken)
  {
    SendValueCount++;
    return base.SendDataAsync(device, command, value, cancellationToken);
  }

  protected override Task UpdateDataAsync(Device device, int? value, CancellationToken cancellationToken)
  {
    UpdateValueCount++;
    return base.UpdateDataAsync(device, value, cancellationToken);
  }
}
