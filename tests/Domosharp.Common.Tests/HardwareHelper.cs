using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Common.Tests;

public static class HardwareHelper
{
  public static IHardware GetFakeHardware(int id, string name, bool enabled, int order, string? configuration, LogLevel logLevel)
  {
    var hardware = Substitute.For<IHardware>();
    hardware.Id.Returns(id);
    hardware.Name.Returns(name);
    hardware.Enabled.Returns(enabled);
    hardware.Order.Returns(order);
    hardware.Configuration.Returns(configuration);
    hardware.LogLevel.Returns(logLevel);
    return hardware;
  }
  public static IHardware GetFakeHardware(
    int id,
    string name,
    bool enabled,
    int order,
    string? configuration,
    LogLevel logLevel,
    HardwareType type)
  {
    return new Dummy()
    {
      Id = id,
      Name = name,
      Enabled = enabled,
      Order = order,
      Configuration = configuration,
      LogLevel = logLevel,
      Type = type
    };
  }

  public static IHardware GetFakeHardware(int id, string name, bool enabled, int order, HardwareType type)
  {
    var hardware = Substitute.For<IHardware>();
    hardware.Id.Returns(id);
    hardware.Name.Returns(name);
    hardware.Enabled.Returns(enabled);
    hardware.Order.Returns(order);
    hardware.Type.Returns(type);
    return hardware;
  }
  public static IHardware GetFakeHardware(HardwareEntity entity)
  {
    return new Dummy()
    {
      Id = entity.Id,
      Name = entity.Name,
      Enabled = entity.Enabled != 0,
      Order = entity.Order,
      Configuration = entity.Configuration,
      LogLevel = (LogLevel)entity.LogLevel,
      Type = (HardwareType)entity.Type,
      LastUpdate = entity.LastUpdate
    };
  }

  public static IHardware GetFakeHardware()
  {
    return new Dummy();
  }

  public static IHardware GetFakeHardware(bool enabled)
  {
    var hardware = Substitute.For<IHardware>();
    hardware.Enabled.Returns(enabled);
    return hardware;
  }
}
