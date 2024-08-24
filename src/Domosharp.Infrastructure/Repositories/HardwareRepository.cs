using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;

using FluentValidation;

using System.Data;

namespace Domosharp.Infrastructure.Repositories;

public class HardwareRepository(IDbConnection connection, IValidator<IHardware> validator) : IHardwareRepository
{
  public static void CreateTable(IDbConnection connection)
  {
    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS [Hardware] (
[Id] INTEGER NOT NULL, 
[Name] VARCHAR(200) NOT NULL, 
[Enabled] INTEGER NOT NULL, 
[Type] INTEGER NOT NULL, 
[LogLevel] INTEGER NOT NULL, 
[Order] INTEGER NOT NULL, 
[Configuration] TEXT NULL,
[LastUpdate] DATETIME NOT NULL,
CONSTRAINT Hardware_PK PRIMARY KEY (Id));";
    cmd.ExecuteNonQuery();
  }

  private int GetMaxId()
  {
    var command = connection.CreateCommand();
    command.CommandText = "SELECT MAX(ID) + 1 FROM [Hardware]";
    var id = command.ExecuteScalar();
    if (id is null || id == DBNull.Value)
      return 1;
    return (int)(long)id;
  }

  public async Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default)
  {
    var validationResult = await validator.ValidateAsync(hardware, cancellationToken);
    if (validationResult is not null && !validationResult.IsValid)
    {
      var error = validationResult.Errors[0];
      if (error.ErrorCode == "ArgumentException")
        throw new ArgumentException(error.ErrorMessage, nameof(hardware));
      else
        throw new ArgumentOutOfRangeException(nameof(hardware), error.ErrorMessage);
    }

    await connection.InsertAsync(hardware.MapHardwareToEntity(GetMaxId(), DateTime.UtcNow));
  }

  public Task<bool> DeleteAsync(int hardwareId, CancellationToken cancellationToken = default)
  {
    return connection.DeleteAsync(new HardwareEntity { Id = hardwareId });
  }

  public async Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default)
  {
    var validationResult = await validator.ValidateAsync(hardware, cancellationToken);
    if (validationResult is not null && !validationResult.IsValid)
    {
      var error = validationResult.Errors[0];
      if (error.ErrorCode == "ArgumentException")
        throw new ArgumentException(error.ErrorMessage, nameof(hardware));
      else
        throw new ArgumentOutOfRangeException(nameof(hardware), error.ErrorMessage);
    }

    return await connection.UpdateAsync(hardware.MapHardwareToEntity(hardware.Id, DateTime.UtcNow));
  }

  public async Task<IHardware?> GetAsync(int hardwareId, CancellationToken cancellationToken = default)
  {
    var entity = await connection.GetAsync(new HardwareEntity(hardwareId));
    if (entity is null)
      return null;

    return entity.MapToModel();
  }

  public async Task<IEnumerable<IHardware>> GetListAsync(CancellationToken cancellationToken = default)
  {
    var entities = await connection.FindAsync<HardwareEntity>(statement => statement.WithAlias("Hardware"));
    return entities.Select( HardwareEntityExtensions.MapToModel);
  }
}
