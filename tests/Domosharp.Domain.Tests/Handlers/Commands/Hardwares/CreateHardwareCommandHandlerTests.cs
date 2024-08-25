using Bogus;

using Domosharp.Business.Contracts.Commands.Hardwares;
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

    public CreateHardwareCommandHandler Build()
    {
      return new CreateHardwareCommandHandler(_hardwareRepository, _mainWorker);
    }
  }
}
