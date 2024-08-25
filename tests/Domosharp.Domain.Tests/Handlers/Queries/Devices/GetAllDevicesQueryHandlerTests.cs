using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Devices;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Queries.Devices;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Queries.Devices;

public class GetAllDevicesQueryHandlerTests
{
  [Fact]
  public async Task GetAllDevices_WithHardwareId_ReturnsDevices()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();

    deviceRepository.GetListAsync(1, Arg.Any<CancellationToken>())
        .Returns(_ => [new Device()]);
    var sut = new SutBuilder()
    .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(new GetAllDevicesQuery() { HardwareId = 1 }, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result);

    await deviceRepository.Received(1).GetListAsync(1, Arg.Any<CancellationToken>());
  }

  private class SutBuilder
  {
    private IDeviceRepository _deviceRepository;

    public SutBuilder()
    {
      _deviceRepository = Substitute.For<IDeviceRepository>();
    }


    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _deviceRepository = deviceRepository;
      return this;
    }

    public GetAllDevicesQueryHandler Build()
    {
      return new GetAllDevicesQueryHandler(_deviceRepository);
    }
  }
}
