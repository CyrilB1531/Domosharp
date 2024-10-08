﻿using Dapper;
using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;

using FluentValidation;

using System.Data;
using System.Text;

namespace Domosharp.Infrastructure.Repositories;

public class DeviceRepository(IDbConnection connection, IValidator<Device> validator) : IDeviceRepository
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
  [Value] NUMBER NULL,
  [DeviceIndex] INTEGER NULL,
	CONSTRAINT Hardware_PK PRIMARY KEY (Id),
	CONSTRAINT Device_Hardware_FK FOREIGN KEY (HardwareId) REFERENCES Hardware(Id));";
    cmd.ExecuteNonQuery();
  }

  private int GetMaxId()
  {
    var command = connection.CreateCommand();
    command.CommandText = "SELECT MAX(Id) + 1 FROM [Device]";
    var id = command.ExecuteScalar();
    if (id is null || id == DBNull.Value)
      return 1;
    return (int)(long)id;
  }

  public async Task<Device?> CreateAsync(Device device, CancellationToken cancellationToken = default)
  {
    var validationResult = await validator.ValidateAsync(device, cancellationToken);
    if (validationResult is not null && !validationResult.IsValid)
    {
      var error = validationResult.Errors[0];
      if (error.ErrorCode == "ArgumentException")
        throw new ArgumentException(error.ErrorMessage, nameof(device));
      else
        throw new ArgumentOutOfRangeException(nameof(device), error.ErrorMessage);
    }
    var entity = device.MapToEntity(GetMaxId());
    if (entity.Order == 0)
      entity.Order = entity.Id;
    await connection.InsertAsync(entity);
    return entity.MapToModel();
  }

  public Task<bool> DeleteAsync(int deviceId, CancellationToken cancellationToken = default)
  {
    return connection.DeleteAsync(new DeviceEntity(deviceId));
  }

  public async Task<bool> UpdateAsync(Device device, CancellationToken cancellationToken = default)
  {
    var validationResult = await validator.ValidateAsync(device, cancellationToken);
    if (validationResult is not null && !validationResult.IsValid)
    {
      var error = validationResult.Errors[0];
      if (error.ErrorCode == "ArgumentException")
        throw new ArgumentException(error.ErrorMessage, nameof(device));
      else
        throw new ArgumentOutOfRangeException(nameof(device), error.ErrorMessage);
    }

    return await connection.UpdateAsync(device.MapToEntity(device.Id));
  }

  public async Task<Device?> GetAsync(int id, CancellationToken cancellationToken = default)
  {
    var result = await connection.GetAsync(new DeviceEntity() { Id = id });
    return result?.MapToModel();
  }

  public async Task<IEnumerable<Device>> GetListAsync(int hardwareId, CancellationToken cancellation = default)
  {
    var result = await connection.FindAsync<DeviceEntity>(statement => statement.Where($"{nameof(Device.HardwareId):C} = {nameof(hardwareId):P}").WithParameters(new { hardwareId }));
    return result.OrderBy(a => a.Order).Select(a => a.MapToModel());
  }

  public async Task<IEnumerable<Device>> GetListAsync(bool onlyActives, bool onlyFavorites, CancellationToken cancellation = default)
  {
    var query = new StringBuilder();
    query.Append("SELECT * FROM Device");
    if(onlyActives || onlyFavorites)
    {
      query.Append(" WHERE");
      if (onlyFavorites)
      {
        query.Append(" [Favorite] = 1");
        if (onlyActives)
          query.Append(" AND");
      }
      if (onlyActives)
      {
        query.Append(" [Active] = 1 AND [HardwareId] IN(SELECT HardwareId FROM Hardware WHERE [Enabled] = 1)");
      }
    }

    var result = await connection.QueryAsync<DeviceEntity>(query.ToString());
    return result.OrderBy(a=>a.Order).Select(a => a.MapToModel());
  }
}
