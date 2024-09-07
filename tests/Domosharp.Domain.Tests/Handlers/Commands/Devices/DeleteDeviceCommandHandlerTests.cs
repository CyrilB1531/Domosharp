using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Devices;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Devices;

public class DeleteDeviceCommandHandlerTests
{
  [Fact]
  public async Task Delete_ExistingDevice_ReturnsTrue()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(_ => new Device());
    deviceRepository.DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
      .Returns(true);
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(new DeleteDeviceCommand { Id = 1 }, CancellationToken.None);

    // Assert
    Assert.True(result);
    await deviceRepository.Received(1).GetAsync(Arg.Is(1), Arg.Any<CancellationToken>());
    await deviceRepository.Received(1).DeleteAsync(Arg.Is(1), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Delete_WithUnknownDevice_DoesNotCallsRepositoryAndReturnsFalse()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(_ => (Device?)null);
    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(new DeleteDeviceCommand { Id = 1 }, CancellationToken.None);

    // Assert
    Assert.False(result);
    await deviceRepository.Received(1).GetAsync(Arg.Is(1), Arg.Any<CancellationToken>());
    await deviceRepository.Received(0).DeleteAsync(Arg.Is(1), Arg.Any<CancellationToken>());
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

    public DeleteDeviceCommandHandler Build()
    {
      return new DeleteDeviceCommandHandler(_deviceRepository);
    }
  }
}
