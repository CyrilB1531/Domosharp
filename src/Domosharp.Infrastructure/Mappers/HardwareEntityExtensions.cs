using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

using Microsoft.Extensions.Logging;

namespace Domosharp.Infrastructure.Mappers;

internal static class HardwareEntityExtensions
{
  public static IHardware MapToModel(this HardwareEntity entity) =>
    new Hardware
    {
      Configuration = entity.Configuration,
      Enabled = entity.Enabled != 0,
      Id = entity.Id,
      LogLevel = (LogLevel)entity.LogLevel,
      Name = entity.Name,
      Order = entity.Order,
      Type = (HardwareType)entity.Type,
      LastUpdate = entity.LastUpdate,
    };
}
