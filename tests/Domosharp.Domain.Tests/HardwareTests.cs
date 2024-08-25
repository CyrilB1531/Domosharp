using Bogus;

using Domosharp.Business.Contracts.Models;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Domain.Tests;

public class HardwareTests
{
  [Fact]
  public void CopyTo_ReturnsSameObject()
  {
    var hardware = new Faker<Hardware>().Rules((faker, hardware)=>
    {
      hardware.LogLevel = faker.PickRandom<LogLevel>();
      hardware.Configuration = faker.Random.Words();
      hardware.Enabled = true;
      hardware.Id = faker.Random.Int();
      hardware.Type = faker.PickRandom<HardwareType>();
      hardware.Name = faker.Random.Words();
      hardware.Order = faker.Random.Int();
    }).Generate();

    var result = (IHardware)new Hardware();
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
