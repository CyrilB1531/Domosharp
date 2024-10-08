﻿using Bogus;

using Domosharp.Business.Contracts.Queries.Hardwares;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Queries.Hardwares;
using Domosharp.Common.Tests;

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
    hardwareRepository.GetListAsync(false, Arg.Any<CancellationToken>())
        .Returns(a => [ HardwareHelper.GetFakeHardware(
               faker.Random.Int(1),
               faker.Random.Words(),
               faker.Random.Bool(),
               faker.Random.Int(1),
               null, Microsoft.Extensions.Logging.LogLevel.None)
        ]);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).GetListAsync(false, Arg.Any<CancellationToken>());
    Assert.Single(result);
  }

  [Fact]
  public async Task GetAllHardwaresHandler_WithoutHardware_ReturnsEmptyList()
  {
    // Arrange
    var command = new GetAllHardwaresQuery();

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetListAsync(true, Arg.Any<CancellationToken>())
        .Returns(a => []);

    var sut = new SutBuilder()
        .WithHardwareRepository(hardwareRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    await hardwareRepository.Received(1).GetListAsync(false, Arg.Any<CancellationToken>());
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
