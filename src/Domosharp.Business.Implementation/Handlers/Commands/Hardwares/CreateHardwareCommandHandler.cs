using Domosharp.Business.Contracts;
using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class CreateHardwareCommandHandler(IHardwareRepository hardwareRepository, 
  IHardwareFactory hardwareFactory,
  IMainWorker mainWorker) : IRequestHandler<CreateHardwareCommand, IHardware?>
{
  public async Task<IHardware?> Handle(CreateHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = await hardwareFactory.CreateAsync(new CreateHardwareParams()
    {
      Id = 0,
      Name = request.Name,
      Enabled = request.Enabled,
      Type = request.Type,
      LogLevel = request.LogLevel,
      Order = request.Order,
      Configuration = request.Configuration
    }, cancellationToken);
    if (hardware is null)
      return null;
    await hardwareRepository.CreateAsync(hardware, cancellationToken);
    mainWorker.AddHardware(hardware);
    return hardware;
  }
}
