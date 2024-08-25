﻿using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class CreateHardwareCommandHandler(IHardwareRepository hardwareRepository, IMainWorker mainWorker) : IRequestHandler<CreateHardwareCommand>
{
  public async Task Handle(CreateHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = new Contracts.Models.Hardware()
    {
      Id = 0,
      Name = request.Name,
      Enabled = request.Enabled,
      Type = request.Type,
      LogLevel = request.LogLevel,
      Order = request.Order,
      Configuration = request.Configuration
    };
    await hardwareRepository.CreateAsync(hardware, cancellationToken);
    mainWorker.AddHardware(hardware);
  }
}
