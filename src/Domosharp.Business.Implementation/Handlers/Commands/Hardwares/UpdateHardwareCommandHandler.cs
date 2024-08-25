using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class UpdateHardwareCommandHandler(
    IHardwareRepository hardwareRepository,
    IMainWorker mainWorker
  ) : IRequestHandler<UpdateHardwareCommand, bool>
{
  public async Task<bool> Handle(UpdateHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = await hardwareRepository.GetAsync(request.Id, cancellationToken);
    if (hardware is null)
      return false;

    var hasChanges = false;
    if (hardware.Name != request.Name)
    {
      hardware.Name = request.Name;
      hasChanges = true;
    }
    if (hardware.Enabled != request.Enabled)
    {
      hardware.Enabled = request.Enabled;
      hasChanges = true;
    }
    if (hardware.LogLevel != request.LogLevel)
    {
      hardware.LogLevel = request.LogLevel;
      hasChanges = true;
    }
    if (hardware.Order != request.Order)
    {
      hardware.Order = request.Order;
      hasChanges = true;
    }
    if (hardware.Configuration != request.Configuration)
    {
      hardware.Configuration = request.Configuration;
      hasChanges = true;
    }

    if (!hasChanges)
      return false;

    if (!await hardwareRepository.UpdateAsync(hardware, cancellationToken))
      return false;
    mainWorker.UpdateHardware(hardware);
    return true;
  }
}
