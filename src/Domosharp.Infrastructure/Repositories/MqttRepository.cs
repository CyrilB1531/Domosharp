using Dapper.FastCrud;

using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Mappers;

using System.Data;

namespace Domosharp.Infrastructure.Repositories;

public class MqttRepository(IDbConnection connection, IDomosharpConfiguration configuration) : IMqttRepository, IMqttEntityRepository
{
  public static void CreateTable(IDbConnection connection)
  {
    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS [MqttHardware] (
[Id] INTEGER PRIMARY KEY, 
[Address] VARCHAR(200), 
[Port] INTEGER, 
[Username] VARCHAR(100), 
[Password] VARCHAR(100), 
[UseTLS] INTEGER DEFAULT 0,
[DataTimeout] INTEGER DEFAULT 0);";
    cmd.ExecuteNonQuery();
  }

  private static bool IsMqttHardware(HardwareType type)
  {
    return type is HardwareType.MQTT or HardwareType.MQTTTasmota;
  }

  public async Task CreateAsync(IHardware hardware, CancellationToken cancellationToken = default)
  {
    if (!IsMqttHardware(hardware.Type))
      return;

    await connection.InsertAsync(((IMqttHardware)hardware).MapToMqttEntity(DateTime.UtcNow, configuration.Aes.KeyBytes(), configuration.Aes.IVBytes()));
  }

  public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
  {
    return connection.DeleteAsync(new MqttEntity(id));
  }

  public Task<bool> UpdateAsync(IHardware hardware, CancellationToken cancellationToken = default)
  {
    if (!IsMqttHardware(hardware.Type))
      return Task.FromResult(false);

    return connection.UpdateAsync(((IMqttHardware)hardware).MapToMqttEntity(DateTime.UtcNow, configuration.Aes.KeyBytes(), configuration.Aes.IVBytes()));
  }

  public async Task<MqttEntity?> GetAsync(int id, CancellationToken cancellationToken = default)
  {
    var result = await connection.GetAsync(new MqttEntity(id));
    return result;
  }
}
