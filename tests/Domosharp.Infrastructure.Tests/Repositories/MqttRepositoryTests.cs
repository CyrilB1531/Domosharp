using Bogus;

using Dapper.FastCrud;

using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Models;
using Domosharp.Common.Tests;
using Domosharp.Infrastructure.DBExtensions;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Repositories;
using Domosharp.Infrastructure.Tests.Fakes;

using Microsoft.Extensions.Logging;

using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Domosharp.Infrastructure.Tests.Repositories;

public class MqttRepositoryTests
{
  public MqttRepositoryTests()
  {
    SqlliteConfigExtensions.InitializeMapper();
  }

  private static MqttEntity CreateMqttEntity(string key, string iv)
  {
    var faker = new Faker();
    var password = faker.Internet.Password();
    var entity = new MqttEntity(faker.Random.Int(1))
    {
      Address = faker.Internet.Ip(),
      Port = faker.Internet.Port(),
      Username = faker.Internet.UserName(),
      Password = GetPassword(password, key, iv),
      UseTLS = faker.Random.Int(0, 1)
    };
    return entity;
  }

  private static async Task<MqttEntity> CreateMqttHardwareInDatabaseAsync(IDbConnection connection, string key, string iv)
  {
    var entity = CreateMqttEntity(key, iv);
    await connection.InsertAsync(entity);
    return entity;
  }

  private static void CheckEntity(MqttEntity? device, MqttEntity expected)
  {
    Assert.NotNull(device);
    Assert.Equal(expected.Id, device.Id);
    Assert.Equal(expected.Address, device.Address);
    Assert.Equal(expected.Port, device.Port);
    Assert.Equal(expected.Username, device.Username);
    Assert.Equal(expected.Password, device.Password);
    Assert.Equal(expected.UseTLS, device.UseTLS);
  }

  private static IHardware CreateHardwareFromEntity(MqttEntity entity, string key, string iv)
  {
    var faker = new Faker();

    var mqttConfiguration = new MqttConfiguration()
    {
      Address = entity.Address??string.Empty,
      Port = entity.Port,
      Password = RetreivePassword( entity.Password, key, iv),
      UserName = entity.Username,
      UseTLS = true,
      SubscriptionsIn = [faker.Random.Word()],
      SubscriptionsOut = [faker.Random.Word()]
    };
    return new Mqtt(mqttConfiguration, faker.System.FilePath())
    {
      Id = entity.Id,
      Name = faker.Random.Word(),
      Enabled = faker.Random.Bool(),
      Order = faker.Random.Int(1)
    };
  }

  private static IHardware CreateDummyHardwareFromEntity(MqttEntity entity)
  {
    var faker = new Faker();

    return new Dummy()
    {
      Id = entity.Id,
      Name = faker.Random.Word(),
      Enabled = faker.Random.Bool(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Word() + Environment.NewLine + faker.Random.Word(),
      LogLevel = faker.PickRandom<LogLevel>()
    };
  }

  private static string GetPassword(string password, string key, string iv)
  {
    var bKey = Convert.FromBase64String(key);
    var bIV = Convert.FromBase64String(iv);
    var aes = Aes.Create();
    using var memoryStream = new MemoryStream();
    using var encStream = new CryptoStream(memoryStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write);
    var pBytes = Encoding.UTF8.GetBytes(password);
    encStream.Write(pBytes, 0, pBytes.Length);
    encStream.Close();
    return Convert.ToBase64String(memoryStream.ToArray());
  }

  private static string? RetreivePassword(string? password, string key, string iv)
  {
    if (string.IsNullOrEmpty(password))
      return null;
    var bKey = Convert.FromBase64String(key);
    var bIV = Convert.FromBase64String(iv);
    var aes = Aes.Create();
    using var memoryStream = new MemoryStream();
    using var decStream = new CryptoStream(memoryStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write);
    var pBytes = Convert.FromBase64String(password);
    decStream.Write(pBytes, 0, pBytes.Length);
    decStream.Close();
    return Encoding.UTF8.GetString(memoryStream.ToArray());
  }

  [Fact]
  public void CreateTable_SetTableInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    // Act
    MqttRepository.CreateTable(connection);

    // Assert
    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM [MqttHardware]";
    var result = command.ExecuteReader();
    Assert.False(result.Read());
  }

  [Fact]
  public async Task Get_ReturnsDataFromDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var expected1 = await CreateMqttHardwareInDatabaseAsync(connection, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);
    await CreateMqttHardwareInDatabaseAsync(connection, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    // Act
    var result = await sut.Repository.GetAsync(expected1.Id, CancellationToken.None);

    // Assert
    CheckEntity(result, expected1);
  }

  [Fact]
  public async Task Get_WithUnknownId_ReturnsNull()
  {
    // Arrange
    var sut = SutBuilder.Build(FakeDBConnectionFactory.GetConnection());

    var expected = CreateMqttEntity(sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    // Act
    var result = await sut.Repository.GetAsync(expected.Id, CancellationToken.None);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task Insert_WithGoodHardware_CreatesDataInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var expected = CreateMqttEntity(sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    var h = CreateHardwareFromEntity(expected, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    // Act
    await sut.Repository.CreateAsync(h, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new MqttEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task Insert_WithNotMqttHardware_DoNothing()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var expected = CreateMqttEntity(sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    var h = CreateDummyHardwareFromEntity(expected);

    // Act
    await sut.Repository.CreateAsync(h, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new MqttEntity(expected.Id));
    Assert.Null(result);
  }

  [Fact]
  public async Task Update_WithGoodHardware_ChangesDataInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var entity = await CreateMqttHardwareInDatabaseAsync(connection, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);
    var expected = CreateMqttEntity(sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);
    expected.Id = entity.Id;

    var h = CreateHardwareFromEntity(expected, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    // Act
    await sut.Repository.UpdateAsync(h, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new MqttEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task Update_WithNotMqttHardware_DoNothing()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var ex = await CreateMqttHardwareInDatabaseAsync(connection, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);
    var expected = CreateMqttEntity(sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);
    expected.Id = ex.Id;

    var h = CreateDummyHardwareFromEntity(expected);

    // Act
    await sut.Repository.UpdateAsync(h, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new MqttEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, ex);
  }

  [Fact]
  public async Task Delete_WithExistingId_RemovesDataInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    var sut = SutBuilder.Build(connection);

    var expected = await CreateMqttHardwareInDatabaseAsync(connection, sut.Configuration.Aes.Key, sut.Configuration.Aes.IV);

    // Act
    await sut.Repository.DeleteAsync(expected.Id, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new MqttEntity(expected.Id));
    Assert.Null(result);
  }

  private static class SutBuilder
  {
    public static (MqttRepository Repository, IDomosharpConfiguration Configuration) Build(IDbConnection connection)
    {
      MqttRepository.CreateTable(connection);
      var configuration = DomosharpConfigurationBuilder.Build();
      return (new MqttRepository(connection, configuration), configuration);
    }
  }
}
