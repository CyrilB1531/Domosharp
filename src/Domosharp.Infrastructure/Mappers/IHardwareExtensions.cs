using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Mappers;

internal static class IHardwareExtensions
{
  internal static HardwareEntity? MapToEntity(this IHardware? entity, int id, DateTime lastUpdate)
  {
    if (entity is null)
      return null;

    ((Hardware)entity).Id = id;
    ((Hardware)entity).LastUpdate = lastUpdate;
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
}
