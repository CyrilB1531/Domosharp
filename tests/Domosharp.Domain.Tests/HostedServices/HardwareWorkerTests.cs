using Bogus;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.HostedServices;
using Domosharp.Domain.Tests.HostedServices.Data;
using DotNetCore.CAP;
using NSubstitute;

namespace Domosharp.Domain.Tests.HostedServices;

public class HardwareWorkerTests
{
  private class SutBuilder
  {
    private IHardwareServiceFactory _hardwareServiceFactory;

    public SutBuilder()
    {
      _hardwareServiceFactory = Substitute.For<IHardwareServiceFactory>();
    }


    public SutBuilder WithHardwareServiceFactory(IHardwareServiceFactory hardwareServiceFactory)
    {
      _hardwareServiceFactory = hardwareServiceFactory;
      return this;
    }

    public HardwareWorker Build()
    {
      return new HardwareWorker(_hardwareServiceFactory);
    }
  }

  [Fact]
  public async Task ShouldSendValue()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareRepository = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareRepository);

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(1, hardwareRepository.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldNotSendValueIfHardwareIsDisabled()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task ShouldNotSendValueIfDeviceIsDisabled()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var deviceService = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceService, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      Hardware = hardware,
      Active = false
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task ShouldUpdateValue()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(1, hardwareService.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldNotUpdateValueIfHardwareIsDisabled()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task ShouldNotUpdateValueIfDeviceIsDisabled()
  {
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      Hardware = hardware,
      Active = false
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }
  
  [Fact]
  public async Task ShouldSendValueThrowsArgumentExceptionIfHardwareIsNull()
  {
    var sut = new SutBuilder()
        .Build();

    var faker = new Faker();

    var device = new Device
    {
      Hardware = null
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);
    var result = await Assert.ThrowsAsync<ArgumentException>("device", () => sut.SendValueAsync(device, command, value, CancellationToken.None));
    Assert.Equal("Hardware not found (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task ShouldUpdateValueThrowsArgumentExceptionIfHardwareIsNull()
  {
    var sut = new SutBuilder()
        .Build();

    var faker = new Faker();

    var device = new Device
    {
      Hardware = null
    };

    var value = faker.Random.Int(0, 100);
    var result = await Assert.ThrowsAsync<ArgumentException>("device", () => sut.UpdateValueAsync(device, value, CancellationToken.None));
    Assert.Equal("Hardware not found (Parameter 'device')", result.Message);
  }

  [Fact]
  public void ShouldDoWorkThrowsArgumentNullExceptionIfParameterIsNull()
  {
    var sut = new SutBuilder()
        .Build();

    Assert.Throws<ArgumentNullException>("obj", () => sut.DoWork(null));
  }

  [Fact]
  public void ShouldDoWorkThrowsArgumentExceptionIfParameterIsNotAnHardware()
  {
    var sut = new SutBuilder()
        .Build();

    var result = Assert.Throws<ArgumentException>("obj", () => sut.DoWork(string.Empty));
    Assert.Equal("Parameter is not an hardware service (Parameter 'obj')", result.Message);
  }

}
