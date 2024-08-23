using Bogus;

using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.DBExtensions;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;
using Domosharp.Infrastructure.Repositories;
using Domosharp.Infrastructure.Tests.Fakes;
using Domosharp.Infrastructure.Validators;

using FluentValidation;

using Microsoft.Extensions.Logging;

using System.Data;

using static Domosharp.Infrastructure.Tests.Repositories.HardwareRepositoryTests;

namespace Domosharp.Infrastructure.Tests.Repositories;

public class DeviceRepositoryTests
{
  public DeviceRepositoryTests()
  {
    SqlliteConfigExtensions.InitializeMapper();
  }

  [Fact]
  public async Task Create_WithoutDeviceName_ThrowsArgumentException()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = string.Empty;
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = faker.Random.Int(1);
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("device", async () => await sut.CreateAsync(device, CancellationToken.None));
    Assert.Equal("Name cannot be null or empty (Parameter 'device')", result.Message);
  }


  [Fact]
  public async Task Create_WithoutDeviceId_ThrowsArgumentException()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = string.Empty;
        device.Favorite = faker.Random.Bool();
        device.HardwareId = faker.Random.Int(1);
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("device", async () => await sut.CreateAsync(device, CancellationToken.None));
    Assert.Equal("DeviceId cannot be null or empty (Parameter 'device')", result.Message);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(101)]
  public async Task Create_WithBadBatteryLevel_ThrowsArgumentOutOfRangeException(int batteryLevel)
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = batteryLevel;
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = faker.Random.Int(1);
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.CreateAsync(device, CancellationToken.None));
    Assert.Equal("BatteryLevel must be between 0 and 100 (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Create_WithBadSignalLevel_ThrowsArgumentOutOfRangeException()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = 1;
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = faker.Random.Int(1);
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.CreateAsync(device, CancellationToken.None));
    Assert.Equal("SignalLevel must be less than 0 (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Create_WithBadOrder_ThrowsArgumentOutOfRangeException()
  {
    // Arrange
    var sut = new SutBuilder().Build();

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = faker.Random.Int(1);
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = -1;
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.CreateAsync(device, CancellationToken.None));
    Assert.Equal("Order must be greater or equal to 0 (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Create_WithGoodDevice_InsertData()
  {
    // Arrange
    var connection = FakeDBConnectionFactory.GetConnection();
    var sut = new SutBuilder(connection).Build();
    var hardware = GetHardware();
    await connection.InsertAsync(hardware);

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = hardware.Id;
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate();

    // Act
    await sut.CreateAsync(device, CancellationToken.None);

    // Assert
    var selectParams = new
    {
      device.HardwareId,
      device.DeviceId
    };
    var entity = await connection.FindAsync<DeviceEntity>(a => a.Where($"{nameof(DeviceEntity.HardwareId):C} = {nameof(selectParams.HardwareId):P} AND {nameof(DeviceEntity.DeviceId):C} = {nameof(selectParams.DeviceId):P}").WithParameters(selectParams));
    Assert.Single(entity);
    var e = entity.First();
    Assert.Equal(device.Name, e.Name);
    Assert.Equal(device.Active ? 1 : 0, e.Active);
    Assert.Equal(device.BatteryLevel, e.BatteryLevel);
    Assert.Equal(device.Favorite ? 1 : 0, e.Favorite);
    Assert.True(e.LastUpdate > DateTime.UtcNow.AddSeconds(-5));
    Assert.Equal(device.Order, e.Order);
    Assert.Equal(device.Protected ? 1 : 0, e.Protected);
    Assert.Equal(device.SignalLevel, e.SignalLevel);
    Assert.Equal(device.SpecificParameters, e.SpecificParameters);
    Assert.Equal((int)device.Type, e.DeviceType);
  }

  [Fact]
  public async Task Create_TwoDevices_InsertData()
  {
    // Arrange
    var connection = FakeDBConnectionFactory.GetConnection();
    var sut = new SutBuilder(connection).Build();
    var hardware = GetHardware();
    await connection.InsertAsync(hardware);

    var device = new Faker<Device>()
      .Rules((faker, device) =>
      {
        device.SignalLevel = faker.Random.Int(0, 100);
        device.Name = faker.Random.String2(10);
        device.BatteryLevel = faker.Random.Int(0, 100);
        device.SignalLevel = faker.Random.Int(-100, 0);
        device.SpecificParameters = faker.Random.String2(20);
        device.Active = faker.Random.Bool();
        device.DeviceId = faker.Random.String2(10);
        device.Favorite = faker.Random.Bool();
        device.HardwareId = hardware.Id;
        device.Id = faker.Random.Int(1);
        device.LastUpdate = faker.Date.Recent();
        device.Order = faker.Random.Int(1);
        device.Protected = faker.Random.Bool();
        device.Type = faker.PickRandom<DeviceType>();
      })
      .Generate(2).ToArray();

    // Act
    await sut.CreateAsync(device[0], CancellationToken.None);
    await sut.CreateAsync(device[1], CancellationToken.None);

    // Assert
    var selectParams = new
    {
      device[1].HardwareId,
      device[1].DeviceId
    };
    var entity = await connection.FindAsync<DeviceEntity>(a => a.Where($"{nameof(DeviceEntity.HardwareId):C} = {nameof(selectParams.HardwareId):P} AND {nameof(DeviceEntity.DeviceId):C} = {nameof(selectParams.DeviceId):P}").WithParameters(selectParams));
    Assert.Single(entity);
    var e = entity.First();
    Assert.Equal(device[1].Name, e.Name);
    Assert.Equal(device[1].Active ? 1 : 0, e.Active);
    Assert.Equal(device[1].BatteryLevel, e.BatteryLevel);
    Assert.Equal(device[1].Favorite ? 1 : 0, e.Favorite);
    Assert.True(e.LastUpdate > DateTime.UtcNow.AddSeconds(-5));
    Assert.Equal(device[1].Order, e.Order);
    Assert.Equal(device[1].Protected ? 1 : 0, e.Protected);
    Assert.Equal(device[1].SignalLevel, e.SignalLevel);
    Assert.Equal(device[1].SpecificParameters, e.SpecificParameters);
    Assert.Equal((int)device[1].Type, e.DeviceType);
  }

  [Fact]
  public async Task GetDevice_ReturnsDevice()
  {
    var connection = FakeDBConnectionFactory.GetConnection();
    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);

    var expected1 = await CreateDeviceInDatabaseAsync(connection, hardware);

    var result = await sut.GetAsync(expected1.Id, CancellationToken.None);

    Assert.NotNull(result);

    CheckEntity(result.MapDeviceToEntity(), expected1, true);
  }

  [Fact]
  public async Task Update_WithoutDeviceName_ThrowsArgumentException()
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;
    expected.Name = string.Empty;

    var result = await Assert.ThrowsAsync<ArgumentException>("device", async () => await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None));
    Assert.Equal("Name cannot be null or empty (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Update_WithoutDeviceId_ThrowsArgumentException()
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;
    expected.DeviceId = string.Empty;

    var result = await Assert.ThrowsAsync<ArgumentException>("device", async () => await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None));
    Assert.Equal("DeviceId cannot be null or empty (Parameter 'device')", result.Message);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(101)]
  public async Task Update_WithBadBatteryLevel_ThrowsArgumentOutOfRangeException(int batteryLevel)
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;
    expected.BatteryLevel = batteryLevel;

    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None));
    Assert.Equal("BatteryLevel must be between 0 and 100 (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Update_WithBadSignalLevel_ThrowsArgumentOutOfRangeException()
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;
    expected.SignalLevel = 1;

    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None));
    Assert.Equal("SignalLevel must be less than 0 (Parameter 'device')", result.Message);
  }

  [Fact]
  public async Task Update_WithBadOrder_ThrowsArgumentOutOfRangeException()
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;
    expected.Order = -1;

    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("device", async () => await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None));
    Assert.Equal("Order must be greater or equal to 0 (Parameter 'device')", result.Message);
  }


  [Fact]
  public async Task Update_WithGoodDevice_UpdateData()
  {
    var connection = FakeDBConnectionFactory.GetConnection();

    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var ex = await CreateDeviceInDatabaseAsync(connection, hardware);
    var expected = CreateDeviceEntity(hardware);
    expected.Id = ex.Id;

    var resultA = await sut.UpdateAsync(expected.MapDeviceToDomain(), CancellationToken.None);

    Assert.True(resultA);
    var result = await connection.GetAsync(new DeviceEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task ShouldDeleteDeviceInDatabase()
  {
    var connection = FakeDBConnectionFactory.GetConnection();
    var sut = new SutBuilder(connection).Build();

    var hardware = await CreateHardwareInDatabaseAsync(connection);
    var expected = await CreateDeviceInDatabaseAsync(connection, hardware);


    var resultA = await sut.DeleteAsync(expected.Id, CancellationToken.None);
    Assert.True(resultA);

    var result = await connection.GetAsync(new DeviceEntity(expected.Id));
    Assert.Null(result);
  }

  private static HardwareEntity GetHardware() => new Faker<HardwareEntity>()
      .Rules((faker, hardware) =>
      {
        hardware.LogLevel = (int)faker.PickRandom<LogLevel>();
        hardware.Id = faker.Random.Int(1);
        hardware.Name = faker.Random.String2(10);
        hardware.Configuration = faker.Random.String2(10);
        hardware.Enabled = 1;
        hardware.Order = 0;
        hardware.Type = 1;
      }).Generate();

  private static async Task<DeviceEntity> CreateDeviceInDatabaseAsync(IDbConnection connection, IHardware hardware)
  {
    var entity = CreateDeviceEntity(hardware);

    var command = connection.CreateCommand();
    command.CommandText = "SELECT MAX(Id) + 1 FROM [Device]";
    var id = command.ExecuteScalar();
    if (id is null || id == DBNull.Value)
      entity.Id = 1;
    else
      entity.Id = (int)(long)id;

    await connection.InsertAsync(entity);
    return entity;
  }

  private static async Task<IHardware> CreateHardwareInDatabaseAsync(IDbConnection connection)
  {
    var hardware = GetHardware();
    await connection.InsertAsync(hardware);
    return hardware.MapToModel();
  }

  private static DeviceEntity CreateDeviceEntity(IHardware hardware)
  {
    var faker = new Faker();
    var entity = new DeviceEntity()
    {
      DeviceId = faker.Random.String2(10),
      DeviceType = (int)faker.PickRandom<DeviceType>(),
      HardwareId = hardware.Id,
      Id = faker.Random.Int(1),
      Name = faker.Random.String2(10),
      Active = faker.Random.Int(0, 1),
      Favorite = faker.Random.Int(0, 1),
      SignalLevel = faker.Random.Int(-100, 0),
      BatteryLevel = faker.Random.Int(0, 100),
      SpecificParameters = faker.Random.String2(10),
      LastUpdate = faker.Date.Recent(),
      Order = faker.Random.Int(0),
      Protected = 0,
    };
    return entity;
  }

  private static void CheckEntity(DeviceEntity device, DeviceEntity expected, bool checkDate = false)
  {
    Assert.Equal(expected.Id, device.Id);
    Assert.Equal(expected.Name, device.Name);
    Assert.Equal(expected.HardwareId, device.HardwareId);
    Assert.Equal(expected.DeviceId, device.DeviceId);
    Assert.Equal(expected.DeviceType, device.DeviceType);
    Assert.Equal(expected.Active, device.Active);
    Assert.Equal(expected.Favorite, device.Favorite);
    Assert.Equal(expected.SignalLevel, device.SignalLevel);
    Assert.Equal(expected.BatteryLevel, device.BatteryLevel);
    Assert.Equal(expected.SpecificParameters, device.SpecificParameters);
    if(checkDate)
      Assert.Equal(expected.LastUpdate, device.LastUpdate);
    else
      Assert.True(DateTime.UtcNow.AddSeconds(-1) < device.LastUpdate);
    Assert.Equal(expected.Order, device.Order);
    Assert.Equal(expected.Protected, device.Protected);
  }

  private class SutBuilder
  {
    private readonly IDbConnection _connection;
    private readonly IValidator<Device> _validator;

    public SutBuilder() : this(FakeDBConnectionFactory.GetConnection())
    {
    }

    public SutBuilder(IDbConnection connection)
    {
      _connection = connection;
      _validator = new DeviceValidator();
      CreateTables();
    }

    private void CreateTables()
    {
      HardwareRepository.CreateTable(_connection);
      DeviceRepository.CreateTable(_connection);
    }

    public DeviceRepository Build() => new(_connection, _validator);
  }
}