using Bogus;

using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Devices;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Devices;

public class UpdateDeviceCommandHandlerTests
{
  private static UpdateDeviceCommand GetUpdateDeviceCommand()
  {
    var faker = new Faker();
    return new UpdateDeviceCommand(faker.Random.String2(10))
    {
      Id = faker.Random.Int(1),
      Active = faker.Random.Bool(),
      Type = faker.PickRandom<DeviceType>(),
      Favorite = faker.Random.Bool(),
      Order = faker.Random.Int(1),
      BatteryLevel = faker.Random.Int(0, 100),
      SignalLevel = faker.Random.Int(-100, 0),
      SpecificParameters = faker.Random.Word(),
      Protected = faker.Random.Bool(),
    };
  }

  [Fact]
  public async Task Update_WithChangedDevice_CallsRepositoryAndReturnsTrue()
  {
    // Arrange
    var faker = new Faker();
    var command = GetUpdateDeviceCommand();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(a => new Device
        {
          Id = a.Arg<int>(),
          Active = !command.Active,
          BatteryLevel = command.BatteryLevel + 1,
          SignalLevel = command.SignalLevel - 1,
          LastUpdate = faker.Date.Recent(),
          SpecificParameters = command.SpecificParameters + "1",
          DeviceId = faker.Random.String2(5),
          Favorite = !command.Favorite,
          HardwareId = 1,
          Name = command.Name + "1",
          Order = command.Order + 1,
          Protected = !command.Protected,
          Type = faker.PickRandomWithout(command.Type),
        });
    deviceRepository.UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result);
    await deviceRepository.Received(1).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Update_WithUnchangedDevice_DoesNotCallRepositoryAndReturnsFalse()
  {
    // Arrange
    var faker = new Faker();
    var command = GetUpdateDeviceCommand();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(a => new Device
        {
          Id = a.Arg<int>(),
          Active = command.Active,
          BatteryLevel = command.BatteryLevel,
          SignalLevel = command.SignalLevel,
          LastUpdate = faker.Date.Recent(),
          SpecificParameters = command.SpecificParameters,
          DeviceId = faker.Random.Word(),
          Favorite = command.Favorite,
          HardwareId = faker.Random.Int(1),
          Name = command.Name,
          Order = command.Order,
          Protected = command.Protected,
          Type = command.Type,
        });

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Update_UnknownDevice_ReturnsFalse()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(_ => (Device?)null);

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    var command = GetUpdateDeviceCommand();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(result);
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  public class SutBuilder
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

    public UpdateDeviceCommandHandler Build()
    {
      return new UpdateDeviceCommandHandler(_deviceRepository);
    }
  }
}
