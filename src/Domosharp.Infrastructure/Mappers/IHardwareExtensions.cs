using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Infrastructure.Mappers
{
  internal static class IHardwareExtensions
  {
    internal static HardwareEntity? MapHardwareToEntity(this IHardware? entity)
    {
      if (entity is null)
        return null;

      return new HardwareEntity
      {
        Id = entity.Id,
        Name = entity.Name,
        Enabled = entity.Enabled ? 1 : 0,
        Configuration = entity.Configuration,
        Type = (int)entity.Type,
        LogLevel = (int)entity.LogLevel,
        Order = entity.Order
      };
    }
  }
}
