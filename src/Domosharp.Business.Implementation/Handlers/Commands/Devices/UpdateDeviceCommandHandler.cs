using Domosharp.Business.Contracts.Commands.Devices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Devices;

public class UpdateDeviceCommandHandler(IDeviceRepository deviceRepository) : IRequestHandler<UpdateDeviceCommand, bool>
{
  public async Task<bool> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
  {
    var device = await deviceRepository.GetAsync(request.Id, cancellationToken);
    if (device is null)
      return false;

    var hasChanges = false;

    if (device.Name != request.Name)
    {
      device.Name = request.Name;
      hasChanges = true;
    }

    if (device.Active != request.Active)
    {
      device.Active = request.Active;
      hasChanges = true;
    }

    if (device.Type != request.Type)
    {
      device.Type = request.Type;
      hasChanges = true;
    }

    if (device.Favorite != request.Favorite)
    {
      device.Favorite = request.Favorite;
      hasChanges = true;
    }

    if (device.SignalLevel != request.SignalLevel)
    {
      device.SignalLevel = request.SignalLevel;
      hasChanges = true;
    }

    if (device.BatteryLevel != request.BatteryLevel)
    {
      device.BatteryLevel = request.BatteryLevel;
      hasChanges = true;
    }

    if (device.Order != request.Order)
    {
      device.Order = request.Order;
      hasChanges = true;
    }

    if (device.Protected != request.Protected)
    {
      device.Protected = request.Protected;
      hasChanges = true;
    }

    if (device.SpecificParameters != request.SpecificParameters)
    {
      device.SpecificParameters = request.SpecificParameters;
      hasChanges = true;
    }

    if (!hasChanges)
      return false;
    return await deviceRepository.UpdateAsync(device, cancellationToken);
  }
}
