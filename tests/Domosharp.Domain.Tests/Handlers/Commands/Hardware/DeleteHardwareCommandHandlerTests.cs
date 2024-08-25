using Bogus;

using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardware;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardware;

public class DeleteHardwareCommandHandlerTests
{
  [Fact]
  public async Task DeleteHandler_WithGoodHardware_ReturnsTrueAndCallsRepository()
  {
    // Arrange
    var faker = new Faker();

    var command = new DeleteHardwareCommand
    {
      Id = faker.Random.Int(1)
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>())
        .Returns(a => new Business.Contracts.Models.Hardware()
        {
          Id = a.Arg<int>(),
          Name = faker.Random.Words(),
          Enabled = faker.Random.Bool(),
          Order = faker.Random.Int(1)
        });
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(true);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);
    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
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
    hardwareRepository.GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>())
        .Returns((IHardware?)null);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
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
    hardwareRepository.GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>())
        .Returns(a => new Business.Contracts.Models.Hardware()
        {
          Id = a.Arg<int>(),
          Name = faker.Random.Words(),
          Enabled = faker.Random.Bool(),
          Order = faker.Random.Int(1)
        });
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(false);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);

    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
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
    public DeleteHardwareCommandHandler Build()
    {
      return new DeleteHardwareCommandHandler(_hardwareRepository);
    }
  }
}
