using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;

using System.Security.Cryptography;
using System.Text;

namespace Domosharp.Infrastructure.Mappers;

internal static class IHardwareExtensions
{
  internal static HardwareEntity? MapToEntity(this IHardware? entity, int id, DateTime lastUpdate)
  {
    if (entity is null)
      return null;

    entity.Id = id;
    entity.LastUpdate = lastUpdate;
    return new()
    {
      Id = id,
      Name = entity.Name,
      Enabled = entity.Enabled ? 1 : 0,
      Configuration = entity.Configuration,
      Type = (int)entity.Type,
      LogLevel = (int)entity.LogLevel,
      Order = entity.Order,
      LastUpdate = lastUpdate,
    };
  }

  internal static MqttEntity? MapToMqttEntity(this IMqttHardware? entity, DateTime lastUpdate, byte[] key, byte[] iv)
  {
    if (entity is null)
      return null;

    entity.LastUpdate = lastUpdate;
    var config = entity.MqttConfiguration;
    if (config is null)
      return null;

    string? password = null;
    if (!string.IsNullOrEmpty(config.Password))
    {
      using var memoryStream = new MemoryStream();
      Aes aes = Aes.Create();
      using var encStream = new CryptoStream(memoryStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write);
      var pass = Encoding.UTF8.GetBytes(config.Password);
      encStream.Write(pass, 0, pass.Length);
      encStream.Close();
      password = Convert.ToBase64String(memoryStream.ToArray());
    }

    return new()
    {
      Id = entity.Id,
      Address = config.Address,
      Password = password,
      Port = config.Port,
      Username = config.UserName,
      UseTLS = config.UseTLS ? 1 : 0,
    };
  }
}
