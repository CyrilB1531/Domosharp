﻿using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardware;

public class UpdateHardwareCommandHandler(
    IHardwareRepository hardwareRepository
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
    if (hardware.Type != request.Type)
    {
      hardware.Type = request.Type;
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

    return await hardwareRepository.UpdateAsync(hardware, cancellationToken);
  }
}
