using Bogus;

using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.DBExtensions;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;
using Domosharp.Infrastructure.Repositories;
using Domosharp.Infrastructure.Tests.Fakes;
using Domosharp.Infrastructure.Validators;

using FluentValidation;

using Microsoft.Extensions.Logging;

using System.Data;

namespace Domosharp.Infrastructure.Tests.Repositories;

public class HardwareRepositoryTests
{
  public HardwareRepositoryTests()
  {
    SqlliteConfigExtensions.InitializeMapper();
  }

  private static HardwareEntity CreateHardwareEntity()
  {
    var faker = new Faker();
    var entity = new HardwareEntity(faker.Random.Int(1))
    {
      Name = faker.Random.Word(),
      Enabled = 1,
      Type = (int)HardwareType.Dummy,
      LogLevel = (int)faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Word(),
    };
    return entity;
  }

  private static async Task<HardwareEntity> CreateHardwareInDatabaseAsync(IDbConnection connection)
  {
    var entity = CreateHardwareEntity();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT MAX(ID) + 1 FROM [Hardware]";
    var id = command.ExecuteScalar();
    if (id is null || id == DBNull.Value)
      entity.Id = 1;
    else
      entity.Id = (int)(long)id;

    await connection.InsertAsync(entity);
    return entity;
  }

  private static void CheckEntity(HardwareEntity? device, HardwareEntity expected)
  {
    Assert.NotNull(device);
    Assert.Equal(expected.Id, device.Id);
    Assert.Equal(expected.Name, device.Name);
    Assert.Equal(expected.Enabled, device.Enabled);
    Assert.Equal(expected.Type, device.Type);
    Assert.Equal(expected.LogLevel, device.LogLevel);
    Assert.Equal(expected.Order, device.Order);
    Assert.Equal(expected.Configuration, device.Configuration);
  }

  [Fact]
  public void CreateTable_SetTableInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    // Act
    HardwareRepository.CreateTable(connection);
    // Assert
    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM [Hardware]";
    var result = command.ExecuteReader();
    Assert.False(result.Read());
  }

  [Fact]
  public async Task GetHardware_GetDataFromDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    HardwareRepository.CreateTable(connection);

    var expected1 = await CreateHardwareInDatabaseAsync(connection);

    var sut = new SutBuilder()
    .WithIDBConnection(connection)
    .Build();

    // Act
    var result = await sut.GetAsync(expected1.Id, CancellationToken.None);

    // Assert
    Assert.NotNull(result);

    CheckEntity(result.MapToEntity(result.Id, result.LastUpdate), expected1);
  }

  [Fact]
  public async Task GetHardware_WithoutData_ReturnsNull()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();
    HardwareRepository.CreateTable(connection);

    var expected = CreateHardwareEntity();

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
    .Build();

    // Act
    var result = await sut.GetAsync(expected.Id, CancellationToken.None);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task Insert_WithNoHardwareInDatabase_SetIdOne()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var expected = CreateHardwareEntity();

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    Assert.NotNull(h);

    // Act
    await sut.CreateAsync(h, CancellationToken.None);

    // Assert
    Assert.Equal(1, h.Id);
    expected.Id = h.Id;

    var result = await connection.GetAsync(new HardwareEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task Create_WithoutHardwareName_ThrowsArgumentException()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    _ = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    h.Name = string.Empty;

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("hardware", async () => await sut.CreateAsync(h, CancellationToken.None));
    Assert.Equal("Name cannot be null or empty (Parameter 'hardware')", result.Message);
  }

  [Fact]
  public async Task Create_WithoutOrder_ThrowsArgumentOutOfRangeException()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    _ = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    h.Order = -1;

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("hardware", async () => await sut.CreateAsync(h, CancellationToken.None));
    Assert.Equal("Order must be greater or equal to 0 (Parameter 'hardware')", result.Message);
  }

  [Fact]
  public async Task Create_WithGoodHardware_InsertData()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    _ = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    Assert.NotNull(h);

    // Act
    await sut.CreateAsync(h, CancellationToken.None);

    // Assert
    expected.Id = h.Id;

    Assert.Equal(2, expected.Id);

    var result = await connection.GetAsync(new HardwareEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task Update_WithoutHardwareName_ThrowsArgumentException()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var ex = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();
    expected.Id = ex.Id;

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    h.Name = string.Empty;

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentException>("hardware", async () => await sut.UpdateAsync(h, CancellationToken.None));
    Assert.Equal("Name cannot be null or empty (Parameter 'hardware')", result.Message);
  }

  [Fact]
  public async Task Update_WithBadOrder_ThrowsArgumentOutOfRangeException()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var ex = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();
    expected.Id = ex.Id;

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    h.Order = -1;

    // Act & Assert
    var result = await Assert.ThrowsAsync<ArgumentOutOfRangeException>("hardware", async () => await sut.UpdateAsync(h, CancellationToken.None));
    Assert.Equal("Order must be greater or equal to 0 (Parameter 'hardware')", result.Message);
  }

  [Fact]
  public async Task Update_GoodHardware_SetDataInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var ex = await CreateHardwareInDatabaseAsync(connection);
    var expected = CreateHardwareEntity();
    expected.Id = ex.Id;

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var h = expected.MapToModel();
    Assert.NotNull(h);

    // Act
    await sut.UpdateAsync(h, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new HardwareEntity(expected.Id));
    Assert.NotNull(result);

    CheckEntity(result, expected);
  }

  [Fact]
  public async Task Delete_RemoveDataInDatabase()
  {
    // Arrange
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var expected = await CreateHardwareInDatabaseAsync(connection);

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    // Act
    await sut.DeleteAsync(expected.Id, CancellationToken.None);

    // Assert
    var result = await connection.GetAsync(new HardwareEntity(expected.Id));
    Assert.Null(result);
  }

  [Fact]
  public async Task ShouldGetHardwaresFromDatabase()
  {
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var expected1 = await CreateHardwareInDatabaseAsync(connection);
    var expected2 = await CreateHardwareInDatabaseAsync(connection);

    var sut = new SutBuilder()
        .WithIDBConnection(connection)
        .Build();

    var result = await sut.GetListAsync(CancellationToken.None);

    Assert.Equal(2, result.Count());
    Assert.Equal(1, result.Count(a => a is not null && a.Id == expected1.Id));
    Assert.Equal(1, result.Count(a => a is not null && a.Id == expected2.Id));

    var result1 = result.First(a => a is not null && a.Id == expected1.Id);
    var result2 = result.First(a => a is not null && a.Id == expected2.Id);
    CheckEntity(result1.MapToEntity(result1.Id, result1.LastUpdate), expected1);
    CheckEntity(result2.MapToEntity(result2.Id, result2.LastUpdate), expected2);
  }

  [Fact]
  public async Task ShouldGetHardwaresFromDatabaseReturnsEmptyList()
  {
    using var connection = FakeDBConnectionFactory.GetConnection();

    HardwareRepository.CreateTable(connection);

    var sut = new SutBuilder().WithIDBConnection(connection).Build();

    var result = await sut.GetListAsync(CancellationToken.None);

    Assert.Empty(result);
  }

  public class SutBuilder
  {
    private IDbConnection _connection;
    private readonly IValidator<IHardware> _validator;

    public SutBuilder()
    {
      _connection = FakeDBConnectionFactory.GetConnection();
      _validator = new HardwareValidator();
    }

    public SutBuilder WithIDBConnection(IDbConnection connection)
    {
      _connection = connection;
      return this;
    }

    public IHardwareRepository Build()
    {
      return new HardwareRepository(_connection, _validator);
    }
  }
}
