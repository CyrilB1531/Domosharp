using Domosharp.Api.Controllers;
using Domosharp.Api.Models;
using Domosharp.Business.Contracts.Commands.Device;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Device;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using System.Collections.Concurrent;

namespace Domosharp.Api.Tests;

public class DeviceControllerTests
{
  [Fact]
  public async Task ShouldGetDevices()
  {
    var expected = new Device
    {
      Id = 5
    };

    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => [expected]);

    var sut = new SutBuilder().WithMediator(mediator).Build();
    var result = await sut.GetListAsync(1, CancellationToken.None);

    Assert.NotNull(result);
    Assert.IsType<OkObjectResult>(result.Result);
    var values = (IEnumerable<DeviceResponse>)((OkObjectResult)result.Result).Value!;
    Assert.NotNull(values);
    Assert.Single(values);
    Assert.Equal(expected.Id, values.First().Id);

  }

  [Fact]
  public async Task ShouldGetDevicesReturnsNull()
  {
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => []);

    var sut = new SutBuilder().WithMediator(mediator).Build();
    var result = await sut.GetListAsync(1, CancellationToken.None);

    Assert.NotNull(result);
    Assert.IsType<OkObjectResult>(result.Result);
    var values = (IEnumerable<DeviceResponse>)((OkObjectResult)result.Result).Value!;
    Assert.NotNull(values);
    Assert.Empty(values);
  }

  [Fact]
  public async Task ShouldCreateDevice()
  {
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => []);

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
    await sut.AddAsync(request, CancellationToken.None);

    await mediator.Received(1).Send(Arg.Any<CreateDeviceCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ShouldUpdateDevices()
  {
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => []);

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
    await sut.UpdateAsync(request, 1, CancellationToken.None);

    await mediator.Received(1).Send(Arg.Any<UpdateDeviceCommand>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ShouldDeleteDevice()
  {
    var mediator = Substitute.For<IMediator>();
    mediator
        .Send(Arg.Any<GetAllDevicesQuery>(), Arg.Any<CancellationToken>())
        .Returns(_ => []);

    var sut = new SutBuilder().WithMediator(mediator).Build();
    await sut.DeleteAsync(1, CancellationToken.None);

    await mediator.Received(1).Send(Arg.Any<DeleteDeviceCommand>(), Arg.Any<CancellationToken>());
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
