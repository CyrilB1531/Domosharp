using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.HostedServices;
using Domosharp.Infrastructure.Tests.Fakes;
using Domosharp.Infrastructure.Tests.HostedServices.Data;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;

using NSubstitute;

using System.Text;
using System.Text.Json;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class MqttTasmotaServiceTests
{
  [Theory]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 1)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 1)]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 2)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 2)]
  public async Task MqttTasmotaService_WithDevicePayload_CreatesCorrectDevices(RelayType relayType, DeviceType deviceType, int deviceCount)
  {
    // Arrange
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType);

    var internalClientIn = new MqttClientTest();
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(deviceType, device.Type);
      return device;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoveryMessage(internalClientIn, payload);

    // Assert
    await deviceRepository.Received(deviceCount).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 1)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 1)]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 2)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 2)]
  public async Task MqttTasmotaService_WithDevicePayload_UpdatesCorrectDevices(RelayType relayType, DeviceType deviceType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();
    var hardware = Substitute.For<IMqttHardware>();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(hardware, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(hardware, oldPayload, 1)
        {
          Active= true,
        } ,
        new TasmotaDevice(hardware, oldPayload, 2)
        {
          Active= true,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(deviceType, device.Type);
      return true;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoveryMessage(internalClientIn, payload);

    // Assert
    await deviceRepository.Received(deviceCount).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, 1)]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Simple, 2)]
  [InlineData(RelayType.Shutter, 2)]
  public async Task MqttTasmotaService_WithUnactiveDeviceAndDevicePayload_DoesNotUpdateDevices(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();
    var hardware = Substitute.For<IMqttHardware>();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(hardware, oldPayload) { Active = false }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(hardware, oldPayload, 1)
        {
          Active= false,
        } ,
        new TasmotaDevice(hardware, oldPayload, 2)
        {
          Active  = false,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);


    // Act
    await CreateMqttTasmotaDiscoveryMessage(internalClientIn, payload);

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData("OFF", 0)]
  [InlineData("ON", 100)]
  public async Task MqttTasmotaService_GetsOneTelemetryMessage(string value, int expectedValue)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(1, RelayType.Light);

    var internalClientIn = new MqttClientTest();
    var deviceRepository = Substitute.For<IDeviceRepository>();
    var hardware = Substitute.For<IMqttHardware>();
    List<Device> getListResult = [new TasmotaDevice(hardware, oldPayload) { Active = true }];
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(expectedValue, device.Value);
      return true;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder()
      .WithDeviceRepository(deviceRepository)
      .WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    var payload = MqttPayload.GetLightState(value);

    // Act
    await CreateMqttTasmotaMessage(internalClientIn, "tele/" + oldPayload.Topic + "/STATE", payload);

    // Assert
    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData("OFF", 0, "OFF", 0)]
  [InlineData("OFF", 0, "ON", 100)]
  [InlineData("ON", 100, "OFF", 0)]
  [InlineData("ON", 100, "ON", 100)]
  public async Task MqttTasmotaService_GetsTwoTelemetryMessage(string value1, int expectedValue1, string value2, int expectedValue2)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(2, RelayType.Light);

    var internalClientIn = new MqttClientTest();
    var deviceRepository = Substitute.For<IDeviceRepository>();
    var hardware = Substitute.For<IMqttHardware>();
    var getListResult = new List<Device>() {
        new TasmotaDevice(hardware, oldPayload, 1)
        {
          Active= true,
        } ,
        new TasmotaDevice(hardware, oldPayload, 2)
        {
          Active  = true,
        }
      };
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      if (device.Name.EndsWith('1'))
        Assert.Equal(expectedValue1, device.Value);
      else
        Assert.Equal(expectedValue2, device.Value);
      return true;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder()
      .WithDeviceRepository(deviceRepository)
      .WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    var payload = MqttPayload.GetTwoLightsState(value1, value2);

    // Act
    await CreateMqttTasmotaMessage(internalClientIn, "tele/" + oldPayload.Topic + "/STATE", payload);

    // Assert
    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(0, 0)]
  [InlineData(0, 100)]
  [InlineData(100, 0)]
  [InlineData(100, 100)]
  [InlineData(80, 80)]
  public async Task MqttTasmotaService_GetsOneSensorMessage(int position, int target)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(1, RelayType.Shutter);

    var internalClientIn = new MqttClientTest();
    var deviceRepository = Substitute.For<IDeviceRepository>();
    var hardware = Substitute.For<IMqttHardware>();
    List<Device> getListResult = [new TasmotaDevice(hardware, oldPayload) { Active = true, Value = 50 }];
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      if (position == target)
        Assert.Equal(position, device.Value);
      else
        Assert.Equal(50, device.Value);
      return true;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder()
      .WithDeviceRepository(deviceRepository)
      .WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    var payload = MqttPayload.GetSensor(position, target);

    // Act
    await CreateMqttTasmotaMessage(internalClientIn, "tele/" + oldPayload.Topic + "/SENSOR", payload);

    // Assert
    if (target == position)
      await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    else
      await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  private static Task<bool> CreateMqttTasmotaDiscoveryMessage(MqttClientTest client, TasmotaDiscoveryPayload payload)
  {
    return CreateMqttTasmotaMessage(client, $"Tasmota/Discovery/{payload.FullMacAsDeviceId}/config", JsonSerializer.Serialize(payload, JsonExtensions.FullObjectOnDeserializing));
  }

  private static async Task<bool> CreateMqttTasmotaMessage(MqttClientTest client, string topic, string payload)
  {
    var message = await client.HandleReceivedApplicationMessageAsync(
        new MQTTnet.Packets.MqttPublishPacket()
        {
          Topic = topic,
          ContentType = "string",
          PayloadSegment = Encoding.UTF8.GetBytes(payload)
        });
    return message.IsHandled;
  }

  private class SutBuilder
  {
    private readonly Faker _faker = new();
    private readonly ICapPublisher _capPublisher = Substitute.For<ICapPublisher>();
    private IDeviceRepository _deviceRepository = Substitute.For<IDeviceRepository>();
    private IManagedMqttClient _clientIn = Substitute.For<IManagedMqttClient>();
    private readonly IManagedMqttClient _clientOut = Substitute.For<IManagedMqttClient>();
    private readonly MqttConfiguration _mqttConfiguration;
    private readonly IMqttHardware _hardware;
    private readonly ILogger _logger;

    public SutBuilder()
    {
      _mqttConfiguration = new MqttConfiguration()
      {
        SubscriptionsIn = ["Tasmota/Discovery"],
        Address = _faker.Internet.Ip(),
        Port = _faker.Internet.Port(),
      };

      _hardware = Substitute.For<IMqttHardware>();
      _hardware.Type.Returns(HardwareType.MQTTTasmota);
      _hardware.MqttConfiguration.Returns(_mqttConfiguration);
      _logger = NullLogger.Instance;
    }

    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _deviceRepository = deviceRepository;
      return this;
    }

    public SutBuilder WithClientIn(IManagedMqttClient clientIn)
    {
      _clientIn = clientIn;
      return this;
    }

    public async Task<MqttTasmotaService> BuildAsync(CancellationToken cancellationToken)
    {
      var service = new MqttTasmotaService(_capPublisher, _deviceRepository, _clientIn, _clientOut, _hardware, _logger);
      await service.ConnectAsync(cancellationToken);
      return service;
    }
  }
}
