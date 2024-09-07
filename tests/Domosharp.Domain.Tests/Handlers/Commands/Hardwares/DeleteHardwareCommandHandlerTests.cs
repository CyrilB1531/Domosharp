using Bogus;

using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardwares;
using Domosharp.Common.Tests;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardwares;

public class DeleteHardwareCommandHandlerTests
{
  [Fact]
  public async Task DeleteHandler_WithGoodHardware_ReturnsTrueAndCallsRepositoryWithoutMqttRepository()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>())
        .Returns(a => HardwareHelper.GetFakeHardware(a.ArgAt<int>(0),
          faker.Random.Words(),
          faker.Random.Bool(),
          faker.Random.Int(1),
          null,
          LogLevel.None,
          HardwareType.Dummy));
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(true);
    var mqttRepository = Substitute.For<IMqttRepository>();

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMqttRepository(mqttRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);
    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    mainWorker.Received(1).DeleteHardware(Arg.Any<int>());
  }

  [Fact]
  public async Task DeleteHandler_WithErrorOnMqttRepository_ReturnsFalseAndDoesNotCallsRepository()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>())
        .Returns(a => HardwareHelper.GetFakeHardware(a.ArgAt<int>(0),
          faker.Random.Words(),
          faker.Random.Bool(),
          faker.Random.Int(1),
          null,
          LogLevel.None,
          HardwareType.MQTT));
    var mqttRepository = Substitute.For<IMqttRepository>();
    mqttRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(false);

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMqttRepository(mqttRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);
    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>());
    await hardwareRepository.Received(0).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await mqttRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    mainWorker.Received(0).DeleteHardware(Arg.Any<int>());
  }

  [Fact]
  public async Task DeleteHandler_WithGoodHardware_ReturnsTrueAndCallsMqttRepository()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>())
        .Returns(a => HardwareHelper.GetFakeHardware(a.ArgAt<int>(0),
          faker.Random.Words(),
          faker.Random.Bool(),
          faker.Random.Int(1),
          null,
          LogLevel.None,
          HardwareType.MQTTTasmota));
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(true);
    var mqttRepository = Substitute.For<IMqttRepository>();
    mqttRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(true);

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMqttRepository(mqttRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);
    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await mqttRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    mainWorker.Received(1).DeleteHardware(Arg.Any<int>());
  }

  [Fact]
  public async Task DeleteHandler_WithUnknownHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>())
        .Returns((IHardware?)null);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>());
    await hardwareRepository.Received(0).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeleteHandler_WithRepositoryFailure_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>())
        .Returns(a => HardwareHelper.GetFakeHardware(
          a.Arg<int>(),
          faker.Random.Words(),
          faker.Random.Bool(),
          faker.Random.Int(1),
          null,
          LogLevel.None));
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(false);
    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), false, Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).DeleteHardware(Arg.Any<int>());
  }

  private class SutBuilder
  {
    private IHardwareRepository _hardwareRepository;
    private IMqttRepository _mqttRepository;
    private IMainWorker _mainWorker;

    public SutBuilder()
    {
      _hardwareRepository = Substitute.For<IHardwareRepository>();
      _mqttRepository = Substitute.For<IMqttRepository>();
      _mainWorker = Substitute.For<IMainWorker>();
    }

    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public SutBuilder WithMqttRepository(IMqttRepository mqttRepository)
    {
      _mqttRepository = mqttRepository;
      return this;
    }

    public SutBuilder WithMainWorker(IMainWorker mainWorker)
    {
      _mainWorker = mainWorker;
      return this;
    }

    public DeleteHardwareCommandHandler Build()
    {
      return new DeleteHardwareCommandHandler(_hardwareRepository, _mqttRepository, _mainWorker);
    }
  }
}
