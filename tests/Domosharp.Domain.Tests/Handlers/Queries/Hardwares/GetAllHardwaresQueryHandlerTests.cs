using Bogus;

using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Hardwares;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Queries.Hardwares;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Queries.Hardwares;

public class GetAllHardwaresQueryHandlerTests
{
  [Fact]
  public async Task GetAllHardwaresHandler_ReturnsData()
  {
    // Act
    var faker = new Faker();

    var command = new GetAllHardwaresQuery();

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetListAsync(Arg.Any<CancellationToken>())
        .Returns(a => [ new Hardware(){
               Id= faker.Random.Int(1),
               Name= faker.Random.Words(),
               Enabled = faker.Random.Bool(),
             Order =   faker.Random.Int(1) }
        ]);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    Assert.Single(result);
  }

  [Fact]
  public async Task GetAllHardwaresHandler_WithoutHardware_ReturnsEmptyList()
  {
    // Arrange
    var command = new GetAllHardwaresQuery();

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetListAsync(Arg.Any<CancellationToken>())
        .Returns(a => []);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    Assert.Empty(result);
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

    public GetAllHardwaresQueryHandler Build()
    {
      return new GetAllHardwaresQueryHandler(_hardwareRepository);
    }
  }
}
