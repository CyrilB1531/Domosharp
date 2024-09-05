using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.HostedServices;
using Domosharp.Common.Tests;

using DotNetCore.CAP;

using MQTTnet.Extensions.ManagedClient;

using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Domosharp.Infrastructure.Tests.HostedServices;

public class HardwareServiceFactoryTests
{
  private static IHardware CreateHardware(HardwareType type)
  {
    var faker = new Faker();
    return type switch
    {
      HardwareType.Dummy => HardwareHelper.GetFakeHardware(
         faker.Random.Int(1),
         faker.Random.Words(),
         faker.Random.Bool(),
         faker.Random.Int(1),
         type
      ),
      _ => throw new ArgumentException("Unknown type", nameof(type))
    };
  }

  [Fact]
  public void CreateFromHardware_WithDummyType_ReturnsDummyHardware()
  {
    // Arrange
    var hardware = CreateHardware(
        HardwareType.Dummy);

    var sut = new SutBuilder().Build();

    // Act
    var result = sut.CreateFromHardware(hardware);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(typeof(DummyService), result.GetType());
  }

  private class SutBuilder
  {
    private readonly IDeviceRepository _deviceRepository;
    private readonly ICapPublisher _capPublisher;
    private readonly IManagedMqttClient _clientIn;
    private readonly IManagedMqttClient _clientOut;
    private readonly ILogger<HardwareServiceFactory> _logger;

    public SutBuilder()
    {
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _capPublisher = Substitute.For<ICapPublisher>();
      _clientIn = Substitute.For<IManagedMqttClient>();
      _clientOut = Substitute.For<IManagedMqttClient>();
      _logger = new NullLogger<HardwareServiceFactory>();
    }

    public HardwareServiceFactory Build()
    {
      return new HardwareServiceFactory(
        _capPublisher,
      _deviceRepository,
      _clientIn,
      _clientOut,
      _logger);
    }
  }
}
