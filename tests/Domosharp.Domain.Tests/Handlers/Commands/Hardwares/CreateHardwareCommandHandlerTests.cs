using Bogus;
using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardwares;

public class CreateHardwareCommandHandlerTests
{
  [Fact]
  public async Task Create_CallsRepositoryToInsertData()
  {
    // Arrange
    var faker = new Faker();

    var command = new CreateHardwareCommand()
    {
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      Type = faker.PickRandom<HardwareType>(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.UpdateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .Build();

    // Act
    await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).CreateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(1).AddHardware(Arg.Any<IHardware>());
  }

  [Fact]
  public async Task Create_WithUnknownHardware_ReturnsNull()
  {
    // Arrange
    var faker = new Faker();

    var command = new CreateHardwareCommand()
    {
      Name = faker.Random.Words(),
      Enabled = faker.Random.Bool(),
      Type = HardwareType.END,
      LogLevel = faker.PickRandom<LogLevel>(),
      Order = faker.Random.Int(1),
      Configuration = faker.Random.Words()
    };

    var hardwareFactory = Substitute.For<IHardwareFactory>();
    hardwareFactory.CreateAsync(Arg.Any<CreateHardwareParams>(), CancellationToken.None).Returns((IHardware?)null);

    var hardwareRepository = Substitute.For<IHardwareRepository>();

    var mainWorker = Substitute.For<IMainWorker>();

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .WithMainWorker(mainWorker)
        .WithHardwareFactory(hardwareFactory)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(0).CreateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
    mainWorker.Received(0).AddHardware(Arg.Any<IHardware>());
    Assert.Null(result);
  }

  private class SutBuilder
  {
    private IHardwareRepository _hardwareRepository;
    private IHardwareFactory _hardwareFactory;
    private IMainWorker _mainWorker;

    public SutBuilder()
    {
      _hardwareRepository = Substitute.For<IHardwareRepository>();
      _hardwareFactory = Substitute.For<IHardwareFactory>();
      _mainWorker = Substitute.For<IMainWorker>();
    }

    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public SutBuilder WithHardwareFactory(IHardwareFactory hardwareFactory)
    {
      _hardwareFactory = hardwareFactory;
      return this;
    }

    public SutBuilder WithMainWorker(IMainWorker mainWorker)
    {
      _mainWorker = mainWorker;
      return this;
    }

    public CreateHardwareCommandHandler Build()
    {
      return new CreateHardwareCommandHandler(_hardwareRepository, _hardwareFactory, _mainWorker);
    }
  }
}
