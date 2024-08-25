﻿using Bogus;

using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardwares;

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
        .Returns(a => new Hardware()
        {
          Id = a.Arg<int>(),
          Name = faker.Random.Words(),
          Enabled = faker.Random.Bool(),
          Order = faker.Random.Int(1)
        });
    hardwareRepository.DeleteAsync(command.Id, CancellationToken.None).Returns(true);
    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);
    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
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
        .Returns(a => new Hardware()
        {
          Id = a.Arg<int>(),
          Name = faker.Random.Words(),
          Enabled = faker.Random.Bool(),
          Order = faker.Random.Int(1)
        });
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

    await hardwareRepository.Received(1).GetAsync(Arg.Is(command.Id), Arg.Any<CancellationToken>());
    await hardwareRepository.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).DeleteHardware(Arg.Any<int>());
  }

  private class SutBuilder
  {
    private IHardwareRepository _hardwareRepository;
    private IMainWorker _mainWorker;

    public SutBuilder()
    {
      _hardwareRepository = Substitute.For<IHardwareRepository>();
      _mainWorker = Substitute.For<IMainWorker>();
    }

    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public SutBuilder WithMainWorker(IMainWorker mainWorker)
    {
      _mainWorker = mainWorker;
      return this;
    }

    public DeleteHardwareCommandHandler Build()
    {
      return new DeleteHardwareCommandHandler(_hardwareRepository, _mainWorker);
    }
  }
}
