using Bogus;

using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Hardware;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Hardware;

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

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).CreateAsync(Arg.Any<IHardware>(), Arg.Any<CancellationToken>());
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

    public CreateHardwareCommandHandler Build()
    {
      return new CreateHardwareCommandHandler(_hardwareRepository);
    }
  }
}
