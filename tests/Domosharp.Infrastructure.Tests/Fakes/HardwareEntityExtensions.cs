using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;
using Domosharp.Common.Tests;

namespace Domosharp.Infrastructure.Tests.Fakes;

internal static class HardwareEntityExtensions
{
  public static IHardware MapToModel(this HardwareEntity entity) => HardwareHelper.GetFakeHardware(entity);
}
