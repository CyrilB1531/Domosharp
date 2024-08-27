using Bogus;

using Domosharp.Business.Contracts;
using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Models;
using Domosharp.Common.Tests;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Factories;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Repositories;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NSubstitute;

using System.Security.Cryptography;
using System.Text;

namespace Domosharp.Infrastructure.Tests.Factories;

public class HardwareFactoryTests
{
  [Theory]
  [InlineData(HardwareType.MQTT, typeof(Mqtt))]
  [InlineData(HardwareType.MQTTTasmota, typeof(MqttTasmota))]
  [InlineData(HardwareType.Dummy, typeof(Dummy))]
  public async Task Create_GivesNewHardware(HardwareType hardwareType, Type type)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(hardwareType, GetMqttConfiguration());

    var mqttEntity = CreateMqttEntity(hardwareEntity.Id, domosharpConfiguration.Aes.Key, domosharpConfiguration.Aes.IV);

    var sut = new SutBuilder(domosharpConfiguration).WithMqttRepositoryGet(a => mqttEntity).Build();

    var result = await sut.CreateAsync(hardwareEntity, CancellationToken.None);
    Assert.NotNull(result);
    Assert.Equal(type, result.GetType());
  }

  [Theory]
  [InlineData(HardwareType.MQTT, typeof(Mqtt))]
  [InlineData(HardwareType.MQTTTasmota, typeof(MqttTasmota))]
  public async Task Create_WithEntity_ReturnsHardware(HardwareType hardwareType, Type type)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var entities = CreateHardwareEntity(hardwareType, domosharpConfiguration.Aes.Key, domosharpConfiguration.Aes.IV);

    var sut = new SutBuilder(domosharpConfiguration).WithMqttRepositoryGet(_ => entities.MqttEntity).Build();

    var result = await sut.CreateAsync(
        entities.HardwareEntity,
        CancellationToken.None);
    Assert.NotNull(result);
    Assert.Equal(type, result.GetType());
  }

  [Theory]
  [InlineData(HardwareType.MQTT)]
  [InlineData(HardwareType.MQTTTasmota)]
  public async Task Create_WithConfigurationError_ThrowsArgumentException(HardwareType hardwareType)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(hardwareType, null);

    var sut = new SutBuilder(domosharpConfiguration).Build();

    await Assert.ThrowsAsync<ArgumentException>("request", () => _ = sut.CreateAsync(hardwareEntity, CancellationToken.None));
  }

  [Theory]
  [InlineData(HardwareType.MQTT)]
  [InlineData(HardwareType.MQTTTasmota)]
  public async Task Create_WithoutConfiguration_ThrowsArgumentException(HardwareType hardwareType)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(hardwareType, "");

    var sut = new SutBuilder(domosharpConfiguration).Build();

    await Assert.ThrowsAsync<ArgumentException>("request", () => _ = sut.CreateAsync(hardwareEntity, CancellationToken.None));
  }

  [Theory]
  [InlineData(HardwareType.MQTT)]
  [InlineData(HardwareType.MQTTTasmota)]
  public async Task Create_WithPortError_ThrowsArgumentException(HardwareType hardwareType)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(hardwareType, GetMqttConfiguration(0));

    var sut = new SutBuilder(domosharpConfiguration).Build();

    await Assert.ThrowsAsync<ArgumentOutOfRangeException>("request", () => _ = sut.CreateAsync(hardwareEntity, CancellationToken.None));
  }

  [Theory]
  [InlineData(HardwareType.MQTT)]
  [InlineData(HardwareType.MQTTTasmota)]
  public async Task Create_WithPortGreaterThan65535_ThrowsArgumentException(HardwareType hardwareType)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(hardwareType, GetMqttConfiguration(65536));

    var sut = new SutBuilder(domosharpConfiguration).Build();

    await Assert.ThrowsAsync<ArgumentOutOfRangeException>("request", () => _ = sut.CreateAsync(hardwareEntity, CancellationToken.None));
  }

  [Theory]
  [InlineData(HardwareType.MQTT)]
  [InlineData(HardwareType.MQTTTasmota)]
  public async Task CreateMQTTHardware_WithNothingInDatabase_ThrowsArgumentException(HardwareType hardwareType)
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var entities = CreateHardwareEntity(hardwareType, domosharpConfiguration.Aes.Key, domosharpConfiguration.Aes.IV);

    var sut = new SutBuilder(domosharpConfiguration).WithMqttRepositoryGet(a => null).Build();

    var error = await Assert.ThrowsAsync<ArgumentException>("entity", () => _ = sut.CreateAsync(entities.HardwareEntity, CancellationToken.None));
    Assert.Equal("Hardware configuration not found (Parameter 'entity')", error.Message);
  }

  [Fact]
  public async Task Create_WithUnknownType_ReturnsNull()
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var hardwareEntity = CreateCreateHardwareParams(
       HardwareType.END, "Configuration");

    var sut = new SutBuilder(domosharpConfiguration).Build();

    var result = await sut.CreateAsync(hardwareEntity, CancellationToken.None);
    Assert.Null(result);
  }

  [Fact]
  public async Task Create_WithUnknownTypeAndEntity_ReturnsNull()
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var entities = CreateHardwareEntity(HardwareType.END, "Configuration");

    var sut = new SutBuilder(domosharpConfiguration).Build();

    var result = await sut.CreateAsync(
        entities.HardwareEntity,
        CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  public async Task Create_WithEntity_ReturnsDummyHardware()
  {
    var domosharpConfiguration = DomosharpConfigurationBuilder.Build();

    var entities = CreateHardwareEntity(
        HardwareType.Dummy,
        string.Empty);

    var sut = new SutBuilder(domosharpConfiguration).Build();

    var result = await sut.CreateAsync(
        entities.HardwareEntity,
        CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(typeof(Dummy), result.GetType());
  }

  private class SutBuilder(IDomosharpConfiguration domosharpConfiguration)
  {
    private bool _useMqttRepositoryGetHardware = false;
    private Func<int, MqttEntity?>? _mqttRepositoryGetHardwareResult = null;

    public SutBuilder WithMqttRepositoryGet(Func<int, MqttEntity?> result)
    {
      if (result is not null)
      {
        _useMqttRepositoryGetHardware = true;
        _mqttRepositoryGetHardwareResult = result;
      }
      return this;
    }

    public HardwareFactory Build()
    {
      var mqttRepository = Substitute.For<IMqttEntityRepository>();

      if (_useMqttRepositoryGetHardware)
      {
        if (_mqttRepositoryGetHardwareResult is null)
          throw new ApplicationException();

        mqttRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(a => _mqttRepositoryGetHardwareResult(a.ArgAt<int>(0)));

      }

      return new HardwareFactory(
         mqttRepository,
          domosharpConfiguration
         );

    }
  }

  private static MqttEntity CreateMqttEntity(int id, string key, string iv)
  {
    var bKey = Convert.FromBase64String(key);
    var bIV = Convert.FromBase64String(iv);
    var faker = new Faker();
    var password = faker.Internet.Password();
    var aes = Aes.Create();
    using var memoryStream = new MemoryStream();
    using var encStream = new CryptoStream(memoryStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write);
    var pBytes = Encoding.UTF8.GetBytes(password);
    encStream.Write(pBytes, 0, pBytes.Length);
    encStream.Close();
    password = Convert.ToBase64String(memoryStream.ToArray());
    return new MqttEntity
    {
      Id = id,
      Address = faker.Internet.Ip(),
      Password = password,
      Port = faker.Internet.Port(),
      Username = faker.Internet.UserName(),
      UseTLS = faker.Random.Bool() ? 1 : 0
    };
  }

  private static (HardwareEntity HardwareEntity, MqttEntity? MqttEntity) CreateHardwareEntity(HardwareType type, string key, string iv)
  {
    var faker = new Faker();

    var id = faker.Random.Int(1);
    var configuration = CreateMqttEntity(id, key, iv);

    var mqttConfiguration = new MqttConfiguration()
    {
      Address = configuration.Address,
      Password = configuration.Password,
      Port = configuration.Port,
      SubscriptionsIn = [],
      SubscriptionsOut = [],
      UserName = configuration.Username,
      UseTLS = configuration.UseTLS != 0
    };

    return (new HardwareEntity()
    {
      Id = id,
      Name = faker.Random.Words(),
      Enabled = faker.Random.Int(0, 1),
      LogLevel = (int)faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Type = (int)type,
      Configuration = JsonConvert.SerializeObject(mqttConfiguration)
    }, configuration);
  }

  private static (HardwareEntity HardwareEntity, MqttEntity? MqttEntity) CreateHardwareEntity(HardwareType type, string configuration)
  {
    var faker = new Faker();

    var id = faker.Random.Int(1);

    return (new HardwareEntity()
    {
      Id = id,
      Name = faker.Random.Words(),
      Enabled = faker.Random.Int(0, 1),
      LogLevel = (int)faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Type = (int)type,
      Configuration = configuration
    }, null);
  }

  private static CreateHardwareParams CreateCreateHardwareParams(HardwareType type, string? configuration)
  {
    var faker = new Faker();
    return new CreateHardwareParams()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Type = type,
      Configuration = configuration
    };
  }

  private static string GetMqttConfiguration(int? port = null)
  {
    var faker = new Faker();
    return JsonConvert.SerializeObject(new MqttConfiguration()
    {
      Address = faker.Internet.IpAddress().ToString(),
      Port = port??faker.Internet.Port(),
      Password = faker.Internet.Password(),
      UserName = faker.Internet.UserName(),
      UseTLS = faker.Random.Bool(),
      SubscriptionsIn = [faker.Random.Word()],
      SubscriptionsOut = [faker.Random.Word()]
    });
  }

}
