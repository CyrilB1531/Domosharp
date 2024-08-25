using Bogus;

using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardware;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardware;

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
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = new Business.Contracts.Models.Hardware()
    {
      Id = command.Id,
      Name = faker.Random.String2(10),
      Enabled = !command.Enabled,
      Order = command.Order + 1
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, CancellationToken.None)
      .Returns(hardware);
    hardwareRepository.UpdateAsync(Arg.Is<IHardware>(a => a.Id == command.Id), CancellationToken.None).Returns(true);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
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
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardware = new Business.Contracts.Models.Hardware()
    {
      Id = command.Id,
      Name = command.Name,
      Enabled = command.Enabled,
      Order = command.Order,
      Configuration = command.Configuration,
      LogLevel = command.LogLevel,
      Type = command.Type
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, CancellationToken.None)
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
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, CancellationToken.None).Returns((IHardware?)null);

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
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, CancellationToken.None).Returns((IHardware?)new Business.Contracts.Models.Hardware()
    {
      Id = command.Id,
      Name = command.Name,
      Enabled = command.Enabled,
      Order = command.Order
    });
    hardwareRepository.UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>()).Returns(false);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
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
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(command.Id, CancellationToken.None).Returns((IHardware?)new Business.Contracts.Models.Hardware()
    {
      Id = command.Id,
      Name = command.Name,
      Enabled = command.Enabled,
      Order = command.Order
    });

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
  }

  private class SutBuilder
  {
    private IHardwareRepository _hardwareRepository;

    public SutBuilder()
    {
      _hardwareRepository = Substitute.For<IHardwareRepository>();
    }


    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public UpdateHardwareCommandHandler Build()
    {
      return new UpdateHardwareCommandHandler(_hardwareRepository);
    }
  }
}
