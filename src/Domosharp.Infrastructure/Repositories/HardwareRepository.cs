using Dapper.FastCrud;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Mappers;

using System.Data;

namespace Domosharp.Infrastructure.Repositories
{
  public class HardwareRepository(IDbConnection connection) : IHardwareRepository
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
CONSTRAINT Hardware_PK PRIMARY KEY (Id));";
      cmd.ExecuteNonQuery();
    }

    public int GetMaxId()
    {
      var command = connection.CreateCommand();
      command.CommandText = "SELECT MAX(ID) + 1 FROM [Hardware]";
      var id = command.ExecuteScalar();
      if (id is null || id == DBNull.Value)
        return 1;
      return (int)(long)id;
    }

    public Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default)
    {
      hardware.Id = GetMaxId();
      return connection.InsertAsync(hardware.MapHardwareToEntity());
    }

    public Task<bool> DeleteAsync(int hardwareId, CancellationToken cancellationToken = default)
    {
      return connection.DeleteAsync(new HardwareEntity { Id = hardwareId });
    }

    public Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default)
    {
      return connection.UpdateAsync(hardware.MapHardwareToEntity());
    }

    public async Task<IHardware?> GetAsync(int hardwareId, CancellationToken cancellationToken = default)
    {
      var entity = await connection.GetAsync(new HardwareEntity(hardwareId));
      if (entity is null)
        return null;

      return entity.MapToModel();
    }

  }
}
