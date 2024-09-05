using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.HostedServices;
using Domosharp.Infrastructure.Tests.HostedServices.Data;

using NSubstitute;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class TasmotaDeviceServiceTests
{
  [Theory]
  [InlineData("ON", 50, 100)]
  [InlineData("OFF", 50, 0)]
  [InlineData("TOGGLE", 50, 0)]
  [InlineData("TOGGLE", 0, 100)]
  [InlineData("HOLD", 50, 50)]
  public async Task DeviceService_WithTelemetryState_CallsRepository(string state, int previousValue, int expected)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Light)) { Value = previousValue };
    var repository = Substitute.For<IDeviceRepository>();
    repository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
      .Returns(a =>
      {
        Assert.Equal(expected, a.ArgAt<Device>(0).Value);
        return true;
      });
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "STATE", MqttPayload.GetLightState(state), CancellationToken.None);

    // Assert
    await repository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData("")]
  [InlineData("{}")]
  public async Task DeviceService_WithUnknownTelemetryState_DoesNotCallsRepository(string payload)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Light)) { Value = 50 };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "STATE", payload, CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(DeviceType.Blinds)]
  [InlineData(DeviceType.Sensor)]
  public async Task DeviceService_WithNotLightSwitchType_DoesNotCallsRepositoryForValue(DeviceType deviceType)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Light)) { Value = 50, Type = deviceType };
    var repository = Substitute.For<IDeviceRepository>();
    repository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
      .Returns(a =>
      {
        var device = a.ArgAt<Device>(0);
        Assert.Equal(50, device.Value);
        return true;
      });

    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "STATE", MqttPayload.GetLightState("ON"), CancellationToken.None);

    // Assert
    await repository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(DeviceType.LightSwitch)]
  public async Task DeviceService_WithLightSwitchDeviceType_DoesNotCallsRepositoryForValue(DeviceType deviceType)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Light)) { Value = 50, Type = deviceType };
    var repository = Substitute.For<IDeviceRepository>();

    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "SENSOR", MqttPayload.GetSensor(0, 0), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WithIndexedBlindDeviceTypeAndOneShutterInSensor_DoesNotCallsRepositoryForValue()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Shutter)) { Value = 50, Type = DeviceType.Blinds, Index = 2 };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "SENSOR", MqttPayload.GetSensor(0, 0), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task DeviceService_WithTemperatureDeviceTypeAndNoTemperatureInSensor_DoesNotCallsRepositoryForValue(bool useEsp32)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Shutter)) { Value = 50, Type = DeviceType.Sensor };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "SENSOR", MqttPayload.GetSensor(0, 0, useEsp32, false), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WithTemperatureDeviceTypeAndTemperatureInSensor_CallsRepositoryForValue()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Shutter)) { Value = 50, Type = DeviceType.Sensor };
    var repository = Substitute.For<IDeviceRepository>();
    repository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
    .Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(49, device.Value);
      return true;
    });
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.TelemetryTopic + "SENSOR", MqttPayload.GetSensor(0, 0), CancellationToken.None);

    // Assert
    await repository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(1, 0)]
  [InlineData(1, 100)]
  [InlineData(2, 0)]
  [InlineData(2, 100)]
  public async Task DeviceService_WithBlindDeviceAndChangedResultStatePayload_CallsRepositoryForValue(int shutterIndex, int position)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(shutterIndex, RelayType.Shutter)) { Value = 50, Type = DeviceType.Blinds, Index = shutterIndex };
    var repository = Substitute.For<IDeviceRepository>();
    repository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
    .Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(position, device.Value);
      Assert.Equal(shutterIndex, device.Index);
      return true;
    });
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "RESULT", MqttPayload.GetResultState(shutterIndex, position, position), CancellationToken.None);

    // Assert
    await repository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(1, 0)]
  [InlineData(1, 100)]
  [InlineData(2, 0)]
  [InlineData(2, 100)]
  public async Task DeviceService_WithBlindDeviceAndTargetNotReachedResultStatePayload_DoesNotCallsRepositoryForValue(int shutterIndex, int position)
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(shutterIndex, RelayType.Shutter)) { Value = 50, Type = DeviceType.Blinds, Index = shutterIndex };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "RESULT", MqttPayload.GetResultState(shutterIndex, position, position + 1), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WithBlindDeviceAndTargetIsSameValueAsDeviceInResultStatePayload_DoesNotCallsRepositoryForValue()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Shutter)) { Value = 50, Type = DeviceType.Blinds };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "RESULT", MqttPayload.GetResultState(1, 50, 50), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WithNotBlindDeviceAndBlindResultStatePayload_DoesNotCallsRepository()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.None)) { Value = 50, Type = DeviceType.Sensor };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "RESULT", MqttPayload.GetResultState(1, 100, 100), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WitUnknownStateCommand_DoNothing()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.None)) { Value = 50, Type = DeviceType.Sensor };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "TEST", MqttPayload.GetResultState(1, 100, 100), CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeviceService_WithoutPayload_DoNqthing()
  {
    // Arrange
    var device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.None)) { Value = 50, Type = DeviceType.Sensor };
    var repository = Substitute.For<IDeviceRepository>();
    var sut = new SutBuilder().WithDevice(device).WithDeviceRepository(repository).Build();

    // Act
    await sut.HandleAsync(device.StateTopic + "RESULT", string.Empty, CancellationToken.None);

    // Assert
    await repository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  private class SutBuilder
  {
    private TasmotaDevice _device;
    private IDeviceRepository _repository;

    public SutBuilder()
    {

      _device = new TasmotaDevice(Substitute.For<IHardware>(), MqttPayload.GetDevicesPayload(1, RelayType.Light));
      _repository = Substitute.For<IDeviceRepository>();
    }

    public SutBuilder WithDevice(TasmotaDevice device)
    {
      _device = device;
      return this;
    }

    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _repository = deviceRepository;
      return this;
    }

    public TasmotaDeviceService Build() => new(_device, _repository);
  }
}
