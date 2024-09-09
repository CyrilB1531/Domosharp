using Domosharp.Api.Controllers;
using Domosharp.Api.Models;
using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Devices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

namespace Domosharp.Api.Tests;

public class DeviceControllerTests
{
  [Fact]
  public async Task GetDevices_WithDevicesDefined_ReturnsOk()
  {
    // Arrange
    var expected = new Device
    {
      Id = 5
    };

    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => [expected]);

    var sut = new SutBuilder().WithMediator(mediator).Build();

    // Act
    var result = await sut.GetListAsync(1, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<OkObjectResult>(result.Result);
    var values = (IEnumerable<DeviceResponse>)((OkObjectResult)result.Result).Value!;
    Assert.NotNull(values);
    Assert.Single(values);
    Assert.Equal(expected.Id, values.First().Id);

  }

  [Fact]
  public async Task GetDevices1_WithDevicesDefined_ReturnsOk()
  {
    // Arrange
    var expected = new Device
    {
      Id = 5
    };

    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => [expected]);

    var sut = new SutBuilder().WithMediator(mediator).Build();

    // Act
    var result = await sut.GetListAsync(false, false, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<OkObjectResult>(result.Result);
    var values = (IEnumerable<DeviceResponse>)((OkObjectResult)result.Result).Value!;
    Assert.NotNull(values);
    Assert.Single(values);
    Assert.Equal(expected.Id, values.First().Id);

  }

  [Fact]
  public async Task GetDevices_WithoutData_ReturnsNoContent()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => []);

    var sut = new SutBuilder().WithMediator(mediator).Build();

    // Act
    var result = await sut.GetListAsync(1, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<NoContentResult>(result.Result);
  }

  [Fact]
  public async Task Create_WithGoodDevice_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<CreateDeviceCommand>(), Arg.Any<CancellationToken>())
        .Returns(a => new Device());

    var sut = new SutBuilder().WithMediator(mediator).Build();
    var request = new CreateDeviceRequest()
    {
      Active = true,
      BatteryLevel = 1,
      HardwareId = 1,
      SignalLevel = -1,
      SpecificParameters = "Params",
      DeviceId = "Device1234",
      Favorite = true,
      Name = "Device",
      Order = 1,
      Protected = false,
      Type = DeviceType.LightSwitch,
    };

    // Act
    await sut.AddAsync(request, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<CreateDeviceCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Update_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<UpdateDeviceCommand>(), Arg.Any<CancellationToken>())
        .Returns(_ => true);

    var sut = new SutBuilder().WithMediator(mediator).Build();
    var request = new UpdateDeviceRequest
    {
      Active = true,
      BatteryLevel = 1,
      SignalLevel = -1,
      SpecificParameters = "Params",
      Favorite = true,
      Name = "Device",
      Order = 1,
      Protected = false,
      Type = DeviceType.LightSwitch,
    };

    // Act
    var result = await sut.UpdateAsync(request, 1, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<UpdateDeviceCommand>(), Arg.Any<CancellationToken>());
    Assert.IsType<OkResult>(result);
  }

  [Fact]
  public async Task Update_ReturnsBadRequest()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<UpdateDeviceCommand>(), Arg.Any<CancellationToken>())
        .Returns(_ => false);

    var sut = new SutBuilder().WithMediator(mediator).Build();
    var request = new UpdateDeviceRequest
    {
      Active = true,
      BatteryLevel = 1,
      SignalLevel = -1,
      SpecificParameters = "Params",
      Favorite = true,
      Name = "Device",
      Order = 1,
      Protected = false,
      Type = DeviceType.LightSwitch,
    };

    // Act
    var result = await sut.UpdateAsync(request, 1, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<UpdateDeviceCommand>(), Arg.Any<CancellationToken>());
    Assert.IsType<BadRequestResult>(result);
  }

  [Fact]
  public async Task Delete_ReturnsOk()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<DeleteDeviceCommand>(), Arg.Any<CancellationToken>())
        .Returns(_ => true);

    var sut = new SutBuilder().WithMediator(mediator).Build();

    // Act
    var result = await sut.DeleteAsync(1, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<DeleteDeviceCommand>(), Arg.Any<CancellationToken>());
    Assert.IsType<OkResult>(result);
  }

  [Fact]
  public async Task Delete_ReturnsBadRequest()
  {
    // Arrange
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<DeleteDeviceCommand>(), Arg.Any<CancellationToken>())
        .Returns(_ => false);

    var sut = new SutBuilder().WithMediator(mediator).Build();

    // Act
    var result = await sut.DeleteAsync(1, CancellationToken.None);

    // Assert
    await mediator.Received(1).Send(Arg.Any<DeleteDeviceCommand>(), Arg.Any<CancellationToken>());
    Assert.IsType<BadRequestResult>(result);
  }

  public class SutBuilder
  {
    private IMediator _mediator;

    public SutBuilder()
    {
      _mediator = Substitute.For<IMediator>();
    }

    public SutBuilder WithMediator(IMediator mediator)
    {
      _mediator = mediator;
      return this;
    }

    public DeviceController Build()
    {
      return new DeviceController(_mediator);
    }
  }
}
