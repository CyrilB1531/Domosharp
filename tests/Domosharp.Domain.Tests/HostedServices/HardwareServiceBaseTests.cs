using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Domain.Tests.HostedServices.Data;

using DotNetCore.CAP;

using NSubstitute;

namespace Domosharp.Domain.Tests.HostedServices;

public class HardwareBaseServiceTests
{
  [Fact]
  public void ShouldEnqueueMessageReturnsGoodMessage()
  {
    var sut = new SutBuilder().Build();

    var device = new Device();
    var message = new Message(MessageType.SendValue, device, "command", 1);

    sut.EnqueueMessage(message);

    var result = sut.DequeueMessage();

    Assert.NotNull(result);
    Assert.Equal(message.Type, result.Type);
    Assert.Equal(message.Command, result.Command);
    Assert.Equal(message.Value, result.Value);
  }

  [Fact]
  public void ShouldDequeueMessageOnEmptyQueueReturnsNull()
  {
    var sut = new SutBuilder().Build();

    var result = sut.DequeueMessage();

    Assert.Null(result);
  }

  [Fact]
  public void ShouldIsStopRequestedReturnsFalseIsNotStarted()
  {
    var sut = new SutBuilder().Build();

    sut.IsStarted = false;
    sut.IsStopRequested = true;

    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldIsStopRequestedReturnsTrueIsRestartRequestSet()
  {
    var sut = new SutBuilder().Build();

    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = true;

    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldIsStopRequestedReturnsFalse()
  {
    var sut = new SutBuilder().Build();

    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;

    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldIsStopRequestedReturnsTrue()
  {
    var sut = new SutBuilder().Build();

    sut.IsStarted = true;
    sut.IsStopRequested = true;
    sut.IsRestartRequested = false;

    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldStopDoNothingIsNotStarted()
  {
    var sut = new SutBuilder().Build();

    sut.IsStarted = false;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;
    sut.Stop();

    Assert.False(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldStopUpdatesProperties()
  {
    var hardware = new Hardware
    {
      Enabled = true
    };

    var sut = new SutBuilder().WithHardware(hardware).Build();
    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;
    sut.Stop();

    Assert.True(sut.IsStopRequested);
  }

  [Fact]
  public void ShouldRestartUpdatesProperties()
  {
    var hardware = new Hardware
    {
      Enabled = false
    };

    var sut = new SutBuilder().WithHardware(hardware).Build();
    sut.IsStarted = true;
    sut.IsStopRequested = false;
    sut.IsRestartRequested = false;
    sut.Restart();

    Assert.True(sut.IsStopRequested);
    Assert.True(sut.IsRestartRequested);
  }

  [Fact]
  public async Task ShouldCreateDevice()
  {
    var hardware = new Hardware
    {
      Enabled = true
    };

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

    sut.CreateDevice(null, new DeviceEventArgs(device));
    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    Assert.Equal(22, device.Id);
  }

  [Fact]
  public async Task ShouldCreateDeviceFails()
  {
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns((Device?)null);

    var hardware = new Hardware
    {
      Enabled = true
    };
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    sut.CreateDevice(null, new DeviceEventArgs(device));

    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    Assert.Equal(0, device.Id);
  }

  [Fact]
  public async Task ShouldUpdateDevice()
  {
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new Device());
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(true);

    var hardware = new Hardware
    {
      Enabled = true
    };
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    sut.UpdateDevice(null, new DeviceEventArgs(device));

    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ShouldNotUpdateDeviceIfNoDeviceFound()
  {
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Device?)null);

    var hardware = new Hardware
    {
      Enabled = true
    };
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardware(hardware)
        .Build();

    var device = new Device()
    {
      Hardware = hardware
    };

    sut.UpdateDevice(null, new DeviceEventArgs(device));

    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ShouldSendValue()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = true
    };

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
    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(1, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldNotSendValueIfHardwareIsDisabled()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = false
    };

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
    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(0, sut.EnqueueMessageCount);
  }


  [Fact]
  public async Task ShouldNotSendValueIfDeviceIsNotUsed()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = true
    };

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware
    };

    var command = faker.Random.Word();
    var value = faker.Random.Int(0, 100);
    await sut.SendValueAsync(device, command, value, CancellationToken.None);
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldUpdateValue()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = true
    };

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = true
    };
    var value = faker.Random.Int(0, 100);
    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(1, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldNotUpdateValueIfHardwareIsDisabled()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = false
    };

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware
    };

    var value = faker.Random.Int(0, 100);
    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Fact]
  public async Task ShouldNotUpdateValueIfDeviceIsNotUsed()
  {
    var faker = new Faker();

    var hardware = new Hardware
    {
      Enabled = true
    };

    var sut = new SutBuilder()
        .WithHardware(hardware)
        .Build();

    var device = new Device
    {
      Hardware = hardware,
      Active = false
    };

    var value = faker.Random.Int(0, 100);
    await sut.UpdateValueAsync(device, value, CancellationToken.None);
    Assert.Equal(0, sut.EnqueueMessageCount);
  }

  [Theory]
  [ClassData(typeof(ProcessLoopData))]
  public async Task ShouldDoProcessLoop(IMessage message)
  {
    Assert.NotNull(message.Device.Hardware);

    var sut = new SutBuilder()
        .WithHardware(message.Device.Hardware)
        .Build();

    sut.DequeueMessageValue = message;

    await sut.ProcessLoop(CancellationToken.None);
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
  public async Task ShouldDoWorkDoNothingIfHardwareIsDisabled()
  {
    var sut = new SutBuilder()
        .Build();

    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(false);
    await sut.DoWorkAsync(CancellationToken.None);

    Assert.Equal(0, sut.ConnectCount);
  }

  [Fact(Timeout = 300)]
  public async Task ShouldDoWorkDoNothingIfHardwareWouldStop()
  {
    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(true);

    var sut = new SutBuilder()
        .WithHardware(hardware)
            .Build();
    sut.IsStopRequested = true;

    await sut.DoWorkAsync(CancellationToken.None);

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
