using Bogus;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.HostedServices;
using DotNetCore.CAP;
using NSubstitute;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class HardwareServiceFactoryTests
{
  private class SutBuilder
  {
    private readonly IDeviceRepository _deviceRepository;
    private readonly ICapPublisher _capPublisher;

    public SutBuilder()
    {
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _capPublisher = Substitute.For<ICapPublisher>();
    }

    public HardwareServiceFactory Build()
    {
      return new HardwareServiceFactory(
        _capPublisher,
      _deviceRepository);
    }
  }

  private static Hardware CreateHardware(HardwareType type)
  {
    var faker = new Faker();
    return type switch
    {
      HardwareType.Dummy => new Hardware(){
        Id = faker.Random.Int(1),
        Name = faker.Random.Words(),
        Enabled = faker.Random.Bool(),
        Order = faker.Random.Int(1),
        Type = type
      },
      _ => throw new ArgumentException("Unknown type", nameof(type))
    };
  }

  [Fact]
  public void ShouldCreateNewDummyHardwareService()
  {
    var hardware = CreateHardware(
        HardwareType.Dummy);

    var sut = new SutBuilder().Build();

    var result = sut.CreateFromHardware(hardware);
    Assert.NotNull(result);
    Assert.Equal(typeof(DummyService), result.GetType());
  }
}
