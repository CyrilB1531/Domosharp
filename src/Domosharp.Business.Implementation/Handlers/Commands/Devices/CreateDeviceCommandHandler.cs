using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Devices;

public class CreateDeviceCommandHandler(IDeviceRepository deviceRepository, IHardwareRepository hardwareRepository) : IRequestHandler<CreateDeviceCommand, Device?>
{
  public async Task<Device?> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
  {
    var hardware = await hardwareRepository.GetAsync(request.HardwareId, cancellationToken);
    if (hardware is null)
      return null;

    var devices = await deviceRepository.GetListAsync(hardware.Id, cancellationToken);
    if (devices.Any(a => a.DeviceId == request.DeviceId))
      return null;

    var device = new Device
    {
      HardwareId = request.HardwareId,
      Active = request.Active,
      BatteryLevel = request.BatteryLevel,
      SignalLevel = request.SignalLevel,
      SpecificParameters = request.SpecificParameters,
      DeviceId = request.DeviceId,
      Favorite = request.Favorite,
      LastUpdate = DateTime.UtcNow,
      Name = request.Name,
      Order = request.Order,
      Protected = request.Protected,
      Type = request.Type,
      Hardware = hardware,
    };

    await deviceRepository.CreateAsync(device, cancellationToken);
    return device;
  }
}
