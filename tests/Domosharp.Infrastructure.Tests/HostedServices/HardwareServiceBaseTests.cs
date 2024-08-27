using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Common.Tests;
using Domosharp.Infrastructure.Tests.HostedServices.Data;

using DotNetCore.CAP;

using NSubstitute;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class HardwareBaseServiceTests
{
  [Fact]
  public void EnqueueMessage_ReturnsGoodMessage()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Device();
    var message = new Message(MessageType.SendValue, device, "command", 1);

    sut.EnqueueMessage(message);

    // Act
    var result = sut.DequeueMessage();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(message.Type, result.Type);
    Assert.Equal(message.Command, result.Command);
    Assert.Equal(message.Value, result.Value);
  }

  [Fact]
  public void DequeueMessage_WithEmptyQueue_ReturnsNull()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    // Act
    var result = sut.DequeueMessage();

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public void IsStopRequested_WithNotStarted_ReturnsFalse()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    sut.IsStarted = false;

    // Act
    sut.IsStopRequested = true;

    // Assert
    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void IsStopRequested_WithIsRestartRequest_ReturnsTrue()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    sut.IsStarted = true;
    sut.IsStopRequested = false;

    // Act
    sut.IsRestartRequested = true;

    // Assert
    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void IsStopRequested_WithStopRequestedNotSet_ReturnsFalse()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    // Act
    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;

    // Assert
    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void IsStopRequested_WithStopRequestedSet_ReturnsTrue()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    // Act
    sut.IsStarted = true;
    sut.IsStopRequested = true;
    sut.IsRestartRequested = false;

    // Assert
    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void Stop_WithNotStarted_DoNothing()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    sut.IsStarted = false;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;

    // Act
    sut.Stop();

    // Assert
    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void Stop_UpdatesProperties()
  {
    // Arrange
    var hardware = HardwareHelper.GetFakeHardware(true);

    var sut = new SutBuilder().WithHardware(hardware).Build();
    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;

    // Act
    sut.Stop();

    // Assert
    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void Restart_UpdatesProperties()
  {
    // Arrange
    var hardware = HardwareHelper.GetFakeHardware(false);

    var sut = new SutBuilder().WithHardware(hardware).Build();
    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;

    // Act
    sut.Restart();

    // Assert
    Assert.True(sut.IsStopRequested);
    Assert.True(sut.IsRestartRequested);
  }

  [Fact]
  public async Task CreateDevice_CallsDeviceRepository()
  {
    // Arrange
    var hardware = HardwareHelper.GetFakeHardware(true);

    var device = new Device()
    {
      Hardware = hardware,
      Id = 22
    };

    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a => a.ArgAt<Device>(0));

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    // Act
    sut.CreateDevice(null, new DeviceEventArgs(device));

    // Assert
    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    Assert.Equal(22, device.Id);
  }

  [Fact]
  public async Task CreateDevice_WithRepositoryFailure_DoNothing()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns((Device?)null);

    var hardware = HardwareHelper.GetFakeHardware(true);
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    // Act
    sut.CreateDevice(null, new DeviceEventArgs(device));

    // Assert
    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    Assert.Equal(0, device.Id);
  }

  [Fact]
  public async Task UpdateDevice_CallsRepository()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new Device());
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(true);

    var hardware = HardwareHelper.GetFakeHardware(true);
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    // Act
    sut.UpdateDevice(null, new DeviceEventArgs(device));

    // Assert
    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateDevice_WithUnkownDevice_DoNothing()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Device?)null);

    var hardware = HardwareHelper.GetFakeHardware(true);
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    // Act
    sut.UpdateDevice(null, new DeviceEventArgs(device));

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task SendValue_EnqueueMessage()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };
    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(1, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task SendValue_WithDisabledHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(false);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, sut.EnqueueMessageCount);
  }


  [Fact]
  public async Task SendValue_WithNotActiveDevice_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);

    // Act
    await sut.SendValueAsync(device, command, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task UpdateValue_EnqueueMessage()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };
    var value = faker.Random.Int(0, 100);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(1, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task UpdateValue_WithDisabledHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(false);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware
    };

    var value = faker.Random.Int(0, 100);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task UpdateValue_WithNotActiveDevice_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var hardware = HardwareHelper.GetFakeHardware(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = false
    };

    var value = faker.Random.Int(0, 100);

    // Act
    await sut.UpdateValueAsync(device, value, CancellationToken.None);

    // Assert
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Theory]
  [ClassData(typeof(ProcessLoopData))]
  public async Task ProcessLoop_SendsMessages(IMessage message)
  {
    // Arrange
    Assert.NotNull(message.Device.Hardware);

    var sut = new SutBuilder()
        .WithHardware(message.Device.Hardware)
        .Build();

    sut.DequeueMessageValue = message;

    // Act
    await sut.ProcessLoop(CancellationToken.None);

    // Assert
    Assert.Equal(1, sut.DequeueMessageCount);

    if (message.Type == MessageType.SendValue)
    {
      Assert.Equal(1, sut.SendValueCount);
      Assert.Equal(0, sut.UpdateValueCount);
    }
    else
    {
      Assert.Equal(0, sut.SendValueCount);
      Assert.Equal(1, sut.UpdateValueCount);
    }
    message.Device.Hardware.ClearReceivedCalls();
  }

  [Fact(Timeout = 300)]
  public async Task DoWork_WithDisabledHardware_DoNothing()
  {
    // Arrange
    var sut = new SutBuilder()
        .Build();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);

    // Act
    await sut.DoWorkAsync(CancellationToken.None);

    // Assert
    Assert.Equal(0, sut.ConnectCount);
  }

  [Fact(Timeout = 300)]
  public async Task DoWork_WithStopRequested_DoNothing()
  {
    // Arrange
    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
            .Build();
    sut.IsStopRequested = true;

    // Act
    await sut.DoWorkAsync(CancellationToken.None);

    // Assert
    Assert.Equal(1, sut.ConnectCount);
    Assert.Equal(1, sut.DisconnectCount);
    Assert.False(sut.IsStarted);
  }

  public class SutBuilder
  {
    private IHardware _hardware;
    private IDeviceRepository _deviceRepository;
    private ICapPublisher _capPublisher;

    public SutBuilder()
    {
      _hardware = Substitute.For<IHardware>();
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _capPublisher = Substitute.For<ICapPublisher>();
    }

    public SutBuilder WithCapPublisher(ICapPublisher capPublisher)
    {
      _capPublisher = capPublisher;
      return this;
    }

    public SutBuilder WithHardware(IHardware hardware)
    {
      _hardware = hardware;
      return this;
    }

    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _deviceRepository = deviceRepository;
      return this;
    }

    public HardwareBaseServiceSutTest Build()
    {
      return new HardwareBaseServiceSutTest(_capPublisher, _deviceRepository, _hardware);
    }
  }
}
