using Bogus;

using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Factories;
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
using System.Text.Json.Nodes;

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
    await CreateMqttTasmotaDiscoveryConfigMessage(internalClientIn, payload);

    // Assert
    await deviceRepository.Received(deviceCount).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 1)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 1)]
  [InlineData(RelayType.Simple, DeviceType.LightSwitch, 2)]
  [InlineData(RelayType.Shutter, DeviceType.Blinds, 2)]
  public async Task MqttTasmotaService_WithDevicesAlreadyPresentAndDevicePayload_CreatesCorrectDevices(RelayType relayType, DeviceType deviceType, int deviceCount)
  {
    // Arrange
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType);

    var internalClientIn = new MqttClientTest();
    var deviceRepository = Substitute.For<IDeviceRepository>();
    var faker = new Faker();
    var mqttConfiguration = new MqttConfiguration()
    {
      SubscriptionsIn = ["Tasmota/Discovery"],
      Address = faker.Internet.Ip(),
      Port = faker.Internet.Port(),
    };

    var hardware = Substitute.For<IMqttHardware>();
    hardware.Id.Returns(faker.Random.Int(1));
    hardware.Type.Returns(HardwareType.MQTTTasmota);
    hardware.MqttConfiguration.Returns(mqttConfiguration);
    List<Device> devices = [new TasmotaDevice(1, MqttPayload.GetDevicesPayload(1, relayType)) { Active = true }];

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(devices);
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      Assert.Equal(deviceType, device.Type);
      return device;
    });
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder()
      .WithDeviceRepository(deviceRepository)
      .WithClientIn(clientIn)
      .WithMqttHardware(hardware)
      .BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoveryConfigMessage(internalClientIn, payload);

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

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
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
    await CreateMqttTasmotaDiscoveryConfigMessage(internalClientIn, payload);

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

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = false }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= false,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = false,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);


    // Act
    await CreateMqttTasmotaDiscoveryConfigMessage(internalClientIn, payload);

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, 1)]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Simple, 2)]
  [InlineData(RelayType.Shutter, 2)]
  public async Task MqttTasmotaService_WithEmptyDevicePayload_DoesNotCreateDevices(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);


    // Act
    await CreateMqttTasmotaDiscoveryConfigMessage(internalClientIn);

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, 1)]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Simple, 2)]
  [InlineData(RelayType.Shutter, 2)]
  public async Task MqttTasmotaService_WithEmptySensorsPayload_DoesNotCreateDevices(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);


    // Act
    await CreateMqttTasmotaDiscoverySensorMessage(internalClientIn, payload.FullMacAsDeviceId, string.Empty);

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Simple, 1, false)]
  [InlineData(RelayType.Simple, 2, false)]
  [InlineData(RelayType.Simple, 1, true)]
  [InlineData(RelayType.Simple, 2, true)]
  public async Task MqttTasmotaService_WithNothingInSensorsPayload_DoesNotCreateDevices(RelayType relayType, int deviceCount, bool withSwitches)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoverySensorMessage(internalClientIn, payload.FullMacAsDeviceId, JsonSerializer.Serialize(CreateDiscoverySensor(withSwitches ? deviceCount : 0, 0, false, false)));

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Shutter, 2)]
  public async Task MqttTasmotaService_WithOnlyShuttersInSensorsPayload_DoesNotCreateDevices(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoverySensorMessage(internalClientIn, payload.FullMacAsDeviceId, JsonSerializer.Serialize(CreateDiscoverySensor(0, deviceCount, false, false)));

    // Assert
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Shutter, 2)]
  [InlineData(RelayType.Light, 1)]
  [InlineData(RelayType.Light, 2)]
  public async Task MqttTasmotaService_WithTemperatureInSensorsPayloadAndDeviceAlreadyPresent_DoesNotCreateDevices(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    List<Device> getListResult = [
      new TasmotaDevice(1, oldPayload) { Active = true, Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds},
      new TasmotaDevice(1, oldPayload) { Active = true, Type = DeviceType.Sensor }
      ];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
          Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds
        } ,
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
          Type= DeviceType.Sensor
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
          Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds
        },
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
          Type= DeviceType.Sensor
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoverySensorMessage(internalClientIn, payload.FullMacAsDeviceId, JsonSerializer.Serialize(CreateDiscoverySensor(0, deviceCount, false, false)));

    // Assert
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData(RelayType.Shutter, 1)]
  [InlineData(RelayType.Shutter, 2)]
  [InlineData(RelayType.Light, 1)]
  [InlineData(RelayType.Light, 2)]
  public async Task MqttTasmotaService_WithTemperatureInSensorsPayload_CreateOneSensorDevice(RelayType relayType, int deviceCount)
  {
    // Arrange
    var oldPayload = MqttPayload.GetDevicesPayload(deviceCount, relayType);
    var payload = MqttPayload.GetDevicesPayload(deviceCount, relayType, oldPayload.FullMacAsDeviceId, oldPayload.DeviceName);

    var internalClientIn = new MqttClientTest();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
      .Returns(a =>
      {
        var device = a.ArgAt<Device>(0);
        Assert.Equal(DeviceType.Sensor, device.Type);
        return device;
      });
    List<Device> getListResult = [
      new TasmotaDevice(1, oldPayload) { Active = true, Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds}
      ];
    if (deviceCount == 2)
    {
      getListResult = [
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
          Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
          Type= relayType==RelayType.Light? DeviceType.LightSwitch : DeviceType.Blinds
        }
      ];
    }

    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    var clientIn = new ManagedMqttClient(internalClientIn, new MqttNetNullLogger());
    await new SutBuilder().WithDeviceRepository(deviceRepository).WithClientIn(clientIn).BuildAsync(CancellationToken.None);

    // Act
    await CreateMqttTasmotaDiscoverySensorMessage(internalClientIn, payload.FullMacAsDeviceId, JsonSerializer.Serialize(CreateDiscoverySensor(0, deviceCount, false, true)));

    // Assert
    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
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
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true }];
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
    var getListResult = new List<Device>() {
        new TasmotaDevice(1, oldPayload, index:1)
        {
          Active= true,
        } ,
        new TasmotaDevice(1, oldPayload, index:2)
        {
          Active  = true,
        }
      };
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(getListResult);
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>()).Returns(a =>
    {
      var device = a.ArgAt<Device>(0);
      if (device.Index == 1)
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
    await deviceRepository.Received(2).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
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
    List<Device> getListResult = [new TasmotaDevice(1, oldPayload) { Active = true, Value = 50 }];
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


  private static JsonObject CreateShutterSensor(int position, int target)
  {
    return new JsonObject(
      [
      new KeyValuePair<string, JsonNode?>("Position", JsonValue.Create(position)),
      new KeyValuePair<string, JsonNode?>("Direction", JsonValue.Create(0)),
      new KeyValuePair<string, JsonNode?>("Target", JsonValue.Create(target)),
      new KeyValuePair<string, JsonNode?>("Tilt", JsonValue.Create(0))
      ]
      );
  }

  private static JsonObject CreateTemperatureSensor(decimal temperature)
  {
    return new JsonObject([
        new KeyValuePair<string, JsonNode?>("Temperature", JsonValue.Create(temperature))
      ]);
  }

  private static JsonObject CreateDiscoverySensor(int switchesCount, int shuttersCount, bool samePositionAndTarget, bool withTemperature)
  {
    var faker = new Faker();

    var sensor = new JsonObject([new KeyValuePair<string, JsonNode?>("Time", JsonValue.Create(DateTime.Now))]);

    for (var i = 1; i <= shuttersCount; i++)
      sensor.Add(new KeyValuePair<string, JsonNode?>("Switch" + i, JsonValue.Create(faker.Random.Bool() ? "ON" : "OFF")));

    for (var i = shuttersCount + 1; i <= shuttersCount + switchesCount; i++)
      sensor.Add(new KeyValuePair<string, JsonNode?>("Switch" + i, JsonValue.Create(faker.Random.Bool() ? "ON" : "OFF")));

    if (withTemperature)
      sensor.Add(new KeyValuePair<string, JsonNode?>("ESP32", CreateTemperatureSensor(Math.Round(faker.Random.Decimal(0, 100), 1))));

    for (var i = 1; i <= shuttersCount; i++)
    {
      var position = faker.Random.Int(0, 100);
      var target = position;
      if (!samePositionAndTarget)
        target = faker.Random.Int(0, 100);
      sensor.Add(new KeyValuePair<string, JsonNode?>("Shutter" + i, CreateShutterSensor(position, target)));
    }
    return sensor;
  }

  private static Task<bool> CreateMqttTasmotaDiscoveryConfigMessage(MqttClientTest client, TasmotaDiscoveryPayload payload)
  {
    return CreateMqttTasmotaMessage(client, $"Tasmota/Discovery/{payload.FullMacAsDeviceId}/config", JsonSerializer.Serialize(payload, JsonExtensions.FullObjectOnDeserializing));
  }

  private static Task<bool> CreateMqttTasmotaDiscoveryConfigMessage(MqttClientTest client)
  {
    return CreateMqttTasmotaMessage(client, $"Tasmota/Discovery/5FC8D2E484/config", string.Empty);
  }

  private static Task<bool> CreateMqttTasmotaDiscoverySensorMessage(MqttClientTest client, string deviceId, string sensorMessage)
  {
    return CreateMqttTasmotaMessage(client, $"Tasmota/Discovery/{deviceId}/sensors", sensorMessage);
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
    private IMqttHardware _hardware;
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

    public SutBuilder WithMqttHardware(IMqttHardware hardware)
    {
      _hardware = hardware;
      return this;
    }

    public async Task<MqttTasmotaService> BuildAsync(CancellationToken cancellationToken)
    {
      var deviceServiceFactory = new DeviceServiceFactory(_deviceRepository);
      var service = new MqttTasmotaService(_capPublisher, _deviceRepository, _clientIn, _clientOut, _hardware, deviceServiceFactory, _logger);
      await service.ConnectAsync(cancellationToken);
      return service;
    }
  }
}
