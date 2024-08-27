using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Common.Tests;

using Microsoft.Extensions.Logging;

namespace Domosharp.Domain.Tests;

public class HardwareTests
{
  [Fact]
  public void CopyTo_ReturnsSameObject()
  {
    var faker = new Faker();
    var hardware = HardwareHelper.GetFakeHardware(
      faker.Random.Int(),
      faker.Random.Words(),
      true,
      faker.Random.Int(),
      faker.Random.Words(),
      faker.PickRandom<LogLevel>(),
      faker.PickRandom<HardwareType>());

    var result = HardwareHelper.GetFakeHardware();
    hardware.CopyTo(ref result);

    Assert.Equal(result.LogLevel, hardware.LogLevel);
    Assert.Equal(result.Configuration, hardware.Configuration);
    Assert.Equal(result.Enabled, hardware.Enabled);
    Assert.Equal(result.Id, hardware.Id);
    Assert.Equal(result.Type, hardware.Type);
    Assert.Equal(result.Name, hardware.Name);
    Assert.Equal(result.Order, hardware.Order);

  }
}
