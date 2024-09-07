using Bogus;

using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.HostedServices;
using Domosharp.Common.Tests.HostedServices;

using DotNetCore.CAP;

using NSubstitute;

namespace Domosharp.Domain.Tests.HostedServices;

public class HardwareWorkerTests
{
  [Fact]
  public async Task SendValue_EnqueueMessage()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = true
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(1, hardwareService.EnqueueMessageCount);
  }

  [Fact]
  public async Task SendValue_WithDisabledHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = true
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task SendValue_WithDisabledDevice_DoNothing()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceService = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceService, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = false
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task UpdateValue_EnqueueMessage()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = true
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(1, hardwareService.EnqueueMessageCount);
  }

  [Fact]
  public async Task UpdateValue_WithDisabledHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = true
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }


  [Fact]
  public async Task UpdateValue_WithDisabledDevice_DoNothing()
  {
    // Arrange
    var faker = new Faker();
    var hardwareFactory = Substitute.For<IHardwareServiceFactory>();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);
    hardware.Id.Returns(1);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns(hardware);

    var deviceRepository = Substitute.For<IDeviceRepository>();
    var capPublisher = Substitute.For<ICapPublisher>();
    var deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();

    var hardwareService = new HardwareBaseServiceSutTest(capPublisher, deviceRepository, deviceServiceFactory, hardware);

    hardwareFactory.CreateFromHardware(Arg.Any<IHardware>())
        .Returns(a => hardwareService);

    var device = new Device
    {
      HardwareId = 1,
      Active = false
    };

    var value = faker.Random.Int(0, 100);

    var sut = new SutBuilder()
        .WithHardwareServiceFactory(hardwareFactory)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    await sut.DoWorkAsync([hardware], CancellationToken.None);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, hardwareService.EnqueueMessageCount);
  }

  [Fact]
  public async Task SendValue_WithNullHardware_ThrowsArgumentException()
  {
    // Arrange
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns((IHardware?)null);
    var sut = new SutBuilder()
      .WithHardwareRepository(hardwareRepository)
      .Build();

    var faker = new Faker();

    var device = new Device
    {
      HardwareId = 1
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("device", () => sut.SendValueAsync(device, command, value, CancellationToken.None));
    Assert.Equal("Hardware not found (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task UpdateValue_WithNullHardware_ThrowsArgumentException()
  {
    // Arrange
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(1, false, Arg.Any<CancellationToken>()).Returns((IHardware?)null);
    var sut = new SutBuilder()
      .WithHardwareRepository(hardwareRepository)
      .Build();

    var faker = new Faker();

    var device = new Device
    {
      HardwareId = 1
    };

    var value = faker.Random.Int(0, 100);

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("device", () => sut.UpdateValueAsync(device, value, CancellationToken.None));
    Assert.Equal("Hardware not found (Parameter 'device')", result.Message);
  }

  [Fact]
  public void DoWork_WithNullParameter_ThrowsArgumentNullException()
  {
    // Arrange
    new SutBuilder().Build();

    // Act & Assert
    Assert.Throws<ArgumentNullException>("obj", () => HardwareWorker.DoWork(null));
  }

  [Fact]
  public void DoWork_WithNotHardwareParameter_ThrowsArgumentException()
  {
    // Arrange
    new SutBuilder().Build();

    // Act & Assert
    var result = Assert.Throws<ArgumentException>("obj", () => HardwareWorker.DoWork(string.Empty));
    Assert.Equal("Parameter is not an hardware service (Parameter 'obj')", result.Message);
  }

  private class SutBuilder
  {
    private IHardwareServiceFactory _hardwareServiceFactory;
    private IHardwareRepository _hardwareRepository;

    public SutBuilder()
    {
      _hardwareServiceFactory = Substitute.For<IHardwareServiceFactory>();
      _hardwareRepository = Substitute.For<IHardwareRepository>();
    }

    public SutBuilder WithHardwareServiceFactory(IHardwareServiceFactory hardwareServiceFactory)
    {
      _hardwareServiceFactory = hardwareServiceFactory;
      return this;
    }

    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public HardwareWorker Build()
    {
      return new HardwareWorker(_hardwareServiceFactory, _hardwareRepository);
    }
  }
}
