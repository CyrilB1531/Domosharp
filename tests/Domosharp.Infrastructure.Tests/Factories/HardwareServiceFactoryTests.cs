using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.HostedServices;

using DotNetCore.CAP;

using MQTTnet.Extensions.ManagedClient;

using NSubstitute;

namespace Domosharp.Infrastructure.Tests.Factories;

public class HardwareServiceFactoryTests
{

  [Theory]
  [InlineData(HardwareType.MQTTTasmota, typeof(MqttTasmotaService))]
  [InlineData(HardwareType.MQTT, typeof(MqttService))]
  [InlineData(HardwareType.Dummy, typeof(DummyService))]
  public void Factory_WithHardware_ReturnsTasmotaService(HardwareType hardwareType, Type type)
  {
    // Arrange
    var sut = new SutBuilder().Build();
    IHardware hardware = hardwareType switch
    {
      HardwareType.MQTTTasmota => new MqttTasmota(new MqttConfiguration(), null),
      HardwareType.MQTT => new Mqtt(new MqttConfiguration(), null),
      _ => new Dummy(),
    };

    // Act
    var result = sut.CreateFromHardware(hardware);

    // Assert
    Assert.IsType(type, result);
  }

  [Fact]
  public void Factory_WithHardware_ThrowsNotImplementedException()
  {
    // Arrange
    var sut = new SutBuilder().Build();
    var hardware = Substitute.For<IHardware>();
    hardware.Type.Returns(HardwareType.System);

    // Act & Assert
    Assert.Throws<NotImplementedException>(() => sut.CreateFromHardware(hardware));
  }

  private class SutBuilder
  {
    private readonly ICapPublisher _capPublisher = Substitute.For<ICapPublisher>();
    private readonly IDeviceRepository _deviceRepository = Substitute.For<IDeviceRepository>();
    private readonly IManagedMqttClient _clientIn = Substitute.For<IManagedMqttClient>();
    private readonly IManagedMqttClient _clientOut = Substitute.For<IManagedMqttClient>();

    public HardwareServiceFactory Build() => new(_capPublisher, _deviceRepository, _clientIn, _clientOut);
  }
}
