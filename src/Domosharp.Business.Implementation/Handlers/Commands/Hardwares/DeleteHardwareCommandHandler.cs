using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class DeleteHardwareCommandHandler(
    IHardwareRepository hardwareRepository,
    IMainWorker mainWorker
  ) : IRequestHandler<DeleteHardwareCommand, bool>
{
  private readonly IHardwareRepository _hardwareRepository = hardwareRepository ?? throw new ArgumentNullException(nameof(hardwareRepository));

  public async Task<bool> Handle(DeleteHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = await _hardwareRepository.GetAsync(request.Id, cancellationToken);
    if (hardware is null)
      return false;

    if (!await _hardwareRepository.DeleteAsync(request.Id, cancellationToken))
      return false;
    mainWorker.DeleteHardware(request.Id);
    return true;
  }
}
