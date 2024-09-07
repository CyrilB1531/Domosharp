using Bogus;

using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.HostedServices;
using Domosharp.Infrastructure.Tests.Fakes;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;

using NSubstitute;

using System.Text;
using System.Text.Json;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class MqttServiceTests
{
  private const string DeviceId = "Tasmota_12345";

  private static async Task<bool> CreateMqttMessage(MqttClientTest client, string topic)
  {
    var message = await client.HandleReceivedApplicationMessageAsync(
        new MQTTnet.Packets.MqttPublishPacket()
        {
          Topic = topic,
          ContentType = "string",
          PayloadSegment = Encoding.UTF8.GetBytes(string.Empty)
        });
    return message.IsHandled;
  }

  [Fact]
  public async Task RecievedMessage_DoNothing()
  {
    // Arrange
    var mqttClientIn = new MqttClientTest();
    var clientIn = new ManagedMqttClient(mqttClientIn, new MqttNetNullLogger());

    var sut = new SutBuilder()
        .WithClientIn(clientIn)
        .Build();

    // Act
    await sut.ConnectAsync(CancellationToken.None);

    var result = await CreateMqttMessage(mqttClientIn, $"in/{DeviceId}/Command");

    await sut.DisconnectAsync(CancellationToken.None);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task SendValue_WithGoodData_IsSent()
  {
    // Arrange
    var mqttClientIn = new MqttClientTest();
    var clientIn = new ManagedMqttClient(mqttClientIn, new MqttNetNullLogger());
    var mqttClientOut = new MqttClientTest();
    var clientOut = new ManagedMqttClient(mqttClientOut, new MqttNetNullLogger());
    var configuration = new MqttConfiguration()
    {
      Address = "127.0.0.1",
      Port = 1883,
      SubscriptionsOut = ["Subscription"]
    };
    var hardware = new Mqtt(configuration, null)
    {
      Id = 1,
      Name = "Test",
      Enabled = true,
      Order = 1,
    };
    var device = new Device
    {
      DeviceId = DeviceId,
      HardwareId = 1,
      Value = 0
    };

    var sut = new SutBuilder()
        .WithClientIn(clientIn)
        .WithClientOut(clientOut)
        .WithHardware(hardware)
        .Build();

    // Act
    await sut.ConnectAsync(CancellationToken.None);

    sut.EnqueueMessage(new Message(MessageType.SendValue, device, "Cmd", 100));

    await sut.ProcessLoop(CancellationToken.None);

    await sut.DisconnectAsync(CancellationToken.None);

    // Assert
    Assert.Equal(1, mqttClientOut.PublishCount);
  }

  [Fact]
  public async Task Update_WithNullValue_DoNothing()
  {
    // Arrange
    var mqttClientIn = new MqttClientTest();
    var clientIn = new ManagedMqttClient(mqttClientIn, new MqttNetNullLogger());
    var mqttClientOut = new MqttClientTest();
    var clientOut = new ManagedMqttClient(mqttClientOut, new MqttNetNullLogger());
    var configuration = new MqttConfiguration()
    {
      Address = "127.0.0.1",
      Port = 1883
    };
    var hardware = new Mqtt(configuration, null)
    {
      Id = 1,
      Name = "Test",
      Enabled = true,
      Order = 1
    };
    var device = new Device
    {
      DeviceId = DeviceId,
      Value = 0,
      HardwareId = 1
    };

    var sut = new SutBuilder()
        .WithClientIn(clientIn)
        .WithClientOut(clientOut)
        .WithHardware(hardware)
        .Build();

    // Act
    await sut.ConnectAsync(CancellationToken.None);

    sut.EnqueueMessage(new Message(MessageType.UpdateValue, device, string.Empty, null));

    await sut.ProcessLoop(CancellationToken.None);

    await sut.DisconnectAsync(CancellationToken.None);

    // Assert
    Assert.Equal(0, device.Value);
  }

  [Fact]
  public async Task Update_WithGoodValue_ChangesDataInDatabase()
  {
    // Arrange
    var mqttClientIn = new MqttClientTest();
    var clientIn = new ManagedMqttClient(mqttClientIn, new MqttNetNullLogger());
    var mqttClientOut = new MqttClientTest();
    var clientOut = new ManagedMqttClient(mqttClientOut, new MqttNetNullLogger());
    var configuration = new MqttConfiguration()
    {
      Address = "127.0.0.1",
      Port = 1883
    };
    var hardware = new Mqtt(configuration, null)
    {
      Id = 1,
      Name = "Test",
      Enabled = true,
      Order = 1
    };
    var device = new Device
    {
      DeviceId = DeviceId,
      Value = 0,
      HardwareId = 1
    };

    var sut = new SutBuilder()
        .WithClientIn(clientIn)
        .WithClientOut(clientOut)
        .WithHardware(hardware)
        .Build();

    // Act
    await sut.ConnectAsync(CancellationToken.None);

    sut.EnqueueMessage(new Message(MessageType.UpdateValue, device, string.Empty, 100));

    await sut.ProcessLoop(CancellationToken.None);

    await sut.DisconnectAsync(CancellationToken.None);

    // Assert
    Assert.Equal(100, device.Value);
  }

  private class SutBuilder
  {
    private readonly ICapPublisher _capPublisher;
    private IManagedMqttClient _clientIn;
    private IManagedMqttClient _clientOut;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceServiceFactory _deviceServiceFactory;
    private readonly ILogger _logger;

    private Mqtt _hardware;

    public SutBuilder()
    {
      _clientIn = Substitute.For<IManagedMqttClient>();
      _clientOut = Substitute.For<IManagedMqttClient>();
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _deviceServiceFactory = Substitute.For<IDeviceServiceFactory>();
      _capPublisher = Substitute.For<ICapPublisher>();
      _logger = NullLogger.Instance;

      var sslCertificate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "server_cert.pem");
      var faker = new Faker();
      var configuration = new MqttConfiguration()
      {
        Address = faker.Internet.Ip(),
        Port = faker.Internet.Port(),
        UseTLS = true,
        UserName = faker.Internet.UserName(),
        Password = faker.Internet.Password()
      };
      _hardware = new Mqtt(configuration, sslCertificate)
      {
        Id = faker.Random.Int(1),
        Name = faker.Random.Words(),
        Enabled = true,
        Order = faker.Random.Int(1),
        Configuration = JsonSerializer.Serialize(configuration),
      };
    }

    public SutBuilder WithClientIn(IManagedMqttClient mqttClient)
    {
      _clientIn = mqttClient;
      return this;
    }

    public SutBuilder WithClientOut(IManagedMqttClient mqttClient)
    {
      _clientOut = mqttClient;
      return this;
    }

    public SutBuilder WithHardware(Mqtt hardware)
    {
      _hardware = hardware;
      return this;
    }

    public MqttService Build()
    {
      return new MqttService(
        _capPublisher,
        _deviceRepository,
        _clientIn,
        _clientOut,
        _hardware,
        _deviceServiceFactory,
        _logger);
    }
  }
}
