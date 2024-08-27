using Bogus;

using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Business.Implementation.Handlers.Commands.Devices;

using NSubstitute;

namespace Domosharp.Domain.Tests.Handlers.Commands.Devices;

public class CreateDeviceCommandHandlerTests
{
  private static CreateDeviceCommand GetCreateDeviceCommand()
  {
    var faker = new Faker();
    return new CreateDeviceCommand(faker.Random.String2(10), faker.Random.String2(10))
    {
      Active = faker.Random.Bool(),
      BatteryLevel = faker.Random.Int(0, 100),
      Favorite = faker.Random.Bool(),
      HardwareId = 1,
      Order = faker.Random.Int(0),
      Protected = faker.Random.Bool(),
      SignalLevel = faker.Random.Int(-100, 0),
      SpecificParameters = faker.Random.String2(10),
      Type = faker.PickRandom<DeviceType>(),
    };
  }

  [Fact]
  public async Task Create_WithGoodDevice_CallsRepository()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetListAsync(1, Arg.Any<CancellationToken>())
        .Returns(_ => []);

    var faker = new Faker();

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(a =>
        {
          var dummy = Substitute.For<IHardware>();
          dummy.Id.Returns(a.ArgAt<int>(0));
          dummy.Name.Returns(faker.Random.Words());
          dummy.Enabled.Returns(faker.Random.Bool());
          return dummy;
        });

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    var command = GetCreateDeviceCommand();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);

    await deviceRepository.Received(1).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CreateDevice_WithExsistingDevice_ReturnsNullAndDoesNotCallRepository()
  {
    // Arrange
    var command = GetCreateDeviceCommand();

    var deviceRepository = Substitute.For<IDeviceRepository>();
    deviceRepository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(a => [new Device() { DeviceId = command.DeviceId, HardwareId = a.ArgAt<int>(0) }]);

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .Build();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.Null(result);
    await deviceRepository.Received(0).UpdateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CreateDevice_WithUnkonwnHardware_ReturnsNullAndDoesNotCallRepository()
  {
    // Arrange
    var deviceRepository = Substitute.For<IDeviceRepository>();

    var hardwareRepository = Substitute.For<IHardwareRepository>();
    hardwareRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns(_ => (IHardware?)null);

    var sut = new SutBuilder()
        .WithDeviceRepository(deviceRepository)
        .WithHardwareRepository(hardwareRepository)
        .Build();

    var command = GetCreateDeviceCommand();

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    Assert.Null(result);
    await deviceRepository.Received(0).CreateAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
  }

  public class SutBuilder
  {
    private IDeviceRepository _deviceRepository;
    private IHardwareRepository _hardwareRepository;

    public SutBuilder()
    {
      _deviceRepository = Substitute.For<IDeviceRepository>();
      _hardwareRepository = Substitute.For<IHardwareRepository>();
    }

    public SutBuilder WithDeviceRepository(IDeviceRepository deviceRepository)
    {
      _deviceRepository = deviceRepository;
      return this;
    }

    public SutBuilder WithHardwareRepository(IHardwareRepository hardwareRepository)
    {
      _hardwareRepository = hardwareRepository;
      return this;
    }

    public CreateDeviceCommandHandler Build()
    {
      return new CreateDeviceCommandHandler(_deviceRepository, _hardwareRepository);
    }
  }
}
