using Domosharp.Business.Contracts.Commands.Device;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Device;

public class DeleteDeviceCommandHandler(IDeviceRepository deviceRepository) : IRequestHandler<DeleteDeviceCommand, bool>
{
  public async Task<bool> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
  {
    var device = await deviceRepository.GetAsync(request.Id, cancellationToken);
    if (device is null)
      return false;

    return await deviceRepository.DeleteAsync(request.Id, cancellationToken);
  }
}
