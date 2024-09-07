using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.HostedServices;

using DotNetCore.CAP;

using NSubstitute;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class DummyServiceTests
{
  [Fact]
  public async Task TestAllCompletedTasks()
  {
    var sut = new SutBuilder().Build();
    await sut.ConnectAsync(CancellationToken.None);
    await sut.DisconnectAsync(CancellationToken.None);
    await sut.SendValueAsync(new Device(), "test", 0, CancellationToken.None);
    await sut.UpdateValueAsync(new Device(), 0, CancellationToken.None);
    Assert.True(true);
  }

  private class SutBuilder
  {
    private readonly ICapPublisher _capPublisher;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceServiceFactory _deviceServiceFactory;
    private readonly IHardware _hardware;

    public SutBuilder()
    {
      _capPublisher = Substitute.For<ICapPublisher>();
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();
      _hardware = Substitute.For<IHardware>();
    }

    public DummyService Build() => new(_capPublisher, _deviceRepository, _deviceServiceFactory, _hardware);
  }
}