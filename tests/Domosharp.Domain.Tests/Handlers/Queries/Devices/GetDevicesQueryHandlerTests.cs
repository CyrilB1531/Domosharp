using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Devices;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Queries.Devices;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Queries.Devices;

public class GetDevicesQueryHandlerTests
{
  [Theory]
  [InlineData(false, false)]
  [InlineData(false, true)]
  [InlineData(true, false)]
  [InlineData(true, true)]
  public async Task GetDevices_WithoutFavoritesNorActives_ReturnsDevices(bool actives, bool favorites)
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();

    deviceRepository.GetListAsync(actives, favorites, Arg.Any<CancellationToken>())
        .Returns(_ => [new Device()]);
    var sut = new SutBuilder()
    .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(new GetDevicesQuery() { 
      OnlyActives = actives, 
      OnlyFavorites = favorites }, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result);

    await deviceRepository.Received(1).GetListAsync(actives, favorites, Arg.Any<CancellationToken>());
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

    public GetDevicesQueryHandler Build()
    {
      return new GetDevicesQueryHandler(_deviceRepository);
    }
  }
}
