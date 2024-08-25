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

  [Fact]
  public async Task Service_OnCreateDevice_CallsCreateDeviceRepository()
  {
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
      .Returns(a => a.ArgAt<Device>(0));
    var sut = new SutBuilder().WithDeviceRepository(deviceRepository).Build();

    sut.CreateDevice(this, new DeviceEventArgs(new Device()));

    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  private class SutBuilder
  {
    private readonly ICapPublisher _capPublisher;
    private IDeviceRepository _deviceRepository;
    private readonly IHardware _hardware;

    public SutBuilder()
    {
      _capPublisher = Substitute.For<ICapPublisher>();
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _hardware = Substitute.For<IHardware>();
    }

    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _deviceRepository = deviceRepository;
      return this;
    }

    public DummyService Build() => new(_capPublisher, _deviceRepository, _hardware);
  }
}