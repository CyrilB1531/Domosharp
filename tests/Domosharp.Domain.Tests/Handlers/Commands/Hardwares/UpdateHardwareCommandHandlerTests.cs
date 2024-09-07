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

public class UpdateHardwareCommandHandlerTests
{
  [Fact]
  public async Task UpdateHandler_ReturnsOk()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = HardwareHelper.GetFakeHardware(command.Id, faker.Random.String2(10), !command.Enabled, command.Order + 1, null, LogLevel.None);

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(hardware);
    hardwareRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(true);
    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(1).UpdateHardware(Arg.Any<IHardware>());
  }

  [Fact]
  public async Task UpdateHandler_ReturnsCallsMqttAndHardwareRepositories()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = HardwareHelper.GetFakeHardware(command.Id, faker.Random.String2(10), !command.Enabled, command.Order + 1, null, LogLevel.None, HardwareType.MQTTTasmota);

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(hardware);
    hardwareRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(true);
    var mqttRepository = Substitute.For<IMqttRepository>();
    mqttRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(true);

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

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    await mqttRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(1).UpdateHardware(Arg.Any<IHardware>());
  }

  [Fact]
  public async Task UpdateHandler_WithErrorInMqtt_ReturnsFalseAndDoesNotCallsHardwareRepositories()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = HardwareHelper.GetFakeHardware(command.Id, faker.Random.String2(10), !command.Enabled, command.Order + 1, null, LogLevel.None, HardwareType.MQTTTasmota);

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(hardware);
    var mqttRepository = Substitute.For<IMqttRepository>();
    mqttRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(false);

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

    await hardwareRepository.Received(0).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    await mqttRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).UpdateHardware(Arg.Any<IHardware>());
  }

  [Fact]
  public async Task UpdateHandler_WithNoChanges_ReturnsFalse()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = HardwareHelper.GetFakeHardware(command.Id, command.Name, command.Enabled, command.Order, command.Configuration, command.LogLevel);

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(hardware);
    hardwareRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(true);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(0).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateHandler_WithUnknownHardware_DoNothing()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None).Returns((IHardware?)null);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act & Assert
    Assert.False(await sut.Handle(command, CancellationToken.None));

    await hardwareRepository.Received(0).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task UpdateHandler_WithUpdateFails_ReturnsFalse()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };
    var returnedHardware = HardwareHelper.GetFakeHardware(
        command.Id,
        command.Name,
        command.Enabled,
        command.Order,
        null,
        LogLevel.None);

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(returnedHardware);
    hardwareRepository.UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>()).Returns(false);
    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).UpdateHardware(Arg.Any<IHardware>());
  }

  [Fact]
  public async Task UpdateHardware_WithRepositoryFails_ReturnsFalse()
  {
    // Arrange
    var faker = new Faker();

    var command = new UpdateHardwareCommand()
    {
      Id = faker.Random.Int(1),
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var returnedHardware = HardwareHelper.GetFakeHardware(
          command.Id,
          command.Name,
          command.Enabled,
          command.Order,
          null,
          LogLevel.None);
    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, true, CancellationToken.None)
      .Returns(returnedHardware);

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).UpdateHardware(Arg.Any<IHardware>());
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

    public UpdateHardwareCommandHandler Build()
    {
      return new UpdateHardwareCommandHandler(_hardwareRepository, _mqttRepository, _mainWorker);
    }
  }
}
