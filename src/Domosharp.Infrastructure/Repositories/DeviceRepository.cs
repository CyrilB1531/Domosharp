using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;

using System.Data;
using System.Data.Common;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Domosharp.Infrastructure.Repositories;

public class DeviceRepository(IDbConnection connection) : IDeviceRepository
{
  public static void CreateTable(IDbConnection connection)
  {
    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS [Device] (
	[Id] INTEGER NOT NULL, 
	[HardwareId] INTEGER NOT NULL, 
	[DeviceId] VARCHAR(100) NOT NULL, 
	[Name] VARCHAR(100) NOT NULL, 
	[Active] INTEGER NOT NULL, 
	[Type] INTEGER NOT NULL, 
	[Favorite] INTEGER  NOT NULL, 
	[SignalLevel] INTEGER  NOT NULL, 
	[BatteryLevel] INTEGER  NOT NULL, 
	[LastUpdate] DATETIME  NOT NULL,
	[Order] INTEGER BIGINT(10)  NOT NULL, 
	[SpecificParameters] TEXT NULL, 
	[Protected] INTEGER  NOT NULL,
	CONSTRAINT Hardware_PK PRIMARY KEY (Id),
	CONSTRAINT Device_Hardware_FK FOREIGN KEY (HardwareId) REFERENCES Hardware(Id));";
    cmd.ExecuteNonQuery();
  }

  public int GetMaxId()
  {
    var command = connection.CreateCommand();
    command.CommandText = "SELECT MAX(Id) + 1 FROM [Device]";
    var id = command.ExecuteScalar();
    if (id is null || id == DBNull.Value)
      return 1;
    return (int)(long)id;
  }

  public async Task CreateAsync(Device device, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(device.Name))
      throw new ArgumentException($"{nameof(device.Name)} cannot be null or empty", nameof(device));
    if (string.IsNullOrWhiteSpace(device.DeviceId))
      throw new ArgumentException($"{nameof(device.DeviceId)} cannot be null or empty", nameof(device));
    if (device.BatteryLevel < 0 || device.BatteryLevel > 100)
      throw new ArgumentOutOfRangeException(nameof(device), $"{nameof(device.BatteryLevel)} must be between 0 and 100");
    if (device.SignalLevel > 0)
      throw new ArgumentOutOfRangeException(nameof(device), $"{nameof(device.SignalLevel)} must be less than 0");
    if (device.Order < 0)
      throw new ArgumentOutOfRangeException(nameof(device), $"{nameof(device.Order)} must be greater or equal to 0");

    device.Id = GetMaxId();
    device.LastUpdate = DateTime.UtcNow;
    await connection.InsertAsync(device.MapDeviceToEntity());
  }

  public Task<bool> DeleteAsync(int deviceId, CancellationToken cancellation = default)
  {
    return connection.DeleteAsync(new DeviceEntity(deviceId));
  }

  public Task<bool> UpdateAsync(Device device, CancellationToken cancellation = default)
  {
    return connection.UpdateAsync(device.MapDeviceToEntity());
  }

  public async Task<Device?> GetAsync(int id, CancellationToken cancellation = default)
  {
    var result = await connection.GetAsync(new DeviceEntity() { Id = id });
    return result?.MapDeviceToDomain();
  }
}
