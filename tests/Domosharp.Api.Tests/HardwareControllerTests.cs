using Bogus;

using Domosharp.Api.Controllers;
using Domosharp.Api.Models;
using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Hardwares;
using Domosharp.Common.Tests;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Domosharp.Api.Tests;

public class HardwareControllerTests
{
  [Fact]
  public async Task GetAllHardwares_ReturnsOneItem()
  {
    // Arrange
    var faker = new Faker();
    var expected = new List<IHardware>{
      HardwareHelper.GetFakeHardware(faker.Random.Int(1),
        faker.Random.String2(10),true, 1, faker.Random.String2(10), LogLevel.None)
        };

    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllHardwaresQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => expected);

    var sut = new HardwareController(mediator);

    // Act
    var result = await sut.GetListAsync(CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<OkObjectResult>(result.Result);
    var values = (IEnumerable<HardwareResponse>)((OkObjectResult)result.Result).Value!;
    Assert.NotNull(values);
    Assert.Single(values);
    Assert.Equal(expected[0].Id, values.First().Id);
  }

  [Fact]
  public async Task Delete_WithGoodData_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<DeleteHardwareCommand>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var sut = new HardwareController(mediator);

    // Act
    var result = await sut.DeleteAsync(1, CancellationToken.None);

    // Assert
    Assert.IsType<OkResult>(result);

    await mediator.Received(1).Send(Arg.Any<DeleteHardwareCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Delete_WithWrongData_ReturnsBadRequest()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<DeleteHardwareCommand>(), Arg.Any<CancellationToken>())
      .Returns(false);

    var sut = new HardwareController(mediator);

    // Act
    var result = await sut.DeleteAsync(1, CancellationToken.None);

    // Assert
    Assert.IsType<BadRequestResult>(result);

    await mediator.Received(1).Send(Arg.Any<DeleteHardwareCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Create_CallsMediatr()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();

    var sut = new HardwareController(mediator);
    var faker = new Faker();

    // Act
    await sut.AddAsync(new CreateHardwareRequest
    {
      Configuration = faker.Random.Word(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Name = faker.Random.Word(),
      Order = faker.Random.Int(1),
      Type = faker.PickRandom<HardwareType>()
    }, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<CreateHardwareCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Update_WithGoodData_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<UpdateHardwareCommand>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var sut = new HardwareController(mediator);
    var faker = new Faker();

    // Act
    var result = await sut.UpadateAsync(new UpdateHardwareRequest
    {
      Configuration = faker.Random.Word(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Name = faker.Random.Word(),
      Order = faker.Random.Int(1)
    }, faker.Random.Int(1), CancellationToken.None);

    // Assert
    Assert.IsType<OkResult>(result);
    await mediator.Received(1).Send(Arg.Any<UpdateHardwareCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Update_WithWrongData_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<UpdateHardwareCommand>(), Arg.Any<CancellationToken>())
      .Returns(false);

    var sut = new HardwareController(mediator);
    var faker = new Faker();

    // Act
    var result = await sut.UpadateAsync(new UpdateHardwareRequest
    {
      Configuration = faker.Random.Word(),
      Enabled = faker.Random.Bool(),
      LogLevel = faker.PickRandom<LogLevel>(),
      Name = faker.Random.Word(),
      Order = faker.Random.Int(1)
    }, faker.Random.Int(1), CancellationToken.None);

    // Assert
    Assert.IsType<BadRequestResult>(result);
    await mediator.Received(1).Send(Arg.Any<UpdateHardwareCommand>(), Arg.Any<CancellationToken>());
  }
}