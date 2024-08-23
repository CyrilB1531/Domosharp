using Bogus;

using Dapper;
using Dapper.FastCrud;
using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.DBExtensions;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Repositories;
using Domosharp.Infrastructure.Tests.Fakes;
using Microsoft.Extensions.Logging;

using System.Data;

namespace Domosharp.Infrastructure.Tests.Repositories
{
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

        private class SutBuilder
        {
            private readonly IDbConnection _connection = FakeDBConnectionFactory.GetConnection();

            public SutBuilder()
            {
                CreateTables();
            }

            public SutBuilder(IDbConnection connection)
            {
                _connection = connection;
                CreateTables();
            }

            private void CreateTables()
            {
                HardwareRepository.CreateTable(_connection);
                DeviceRepository.CreateTable(_connection);
            }

            public DeviceRepository Build() => new(_connection);
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
    }
}