using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Repositories;
using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardware;

public class DeleteHardwareCommandHandler(
    IHardwareRepository hardwareRepository
  ) : IRequestHandler<DeleteHardwareCommand, bool>
{
  private readonly IHardwareRepository _hardwareRepository = hardwareRepository ?? throw new ArgumentNullException(nameof(hardwareRepository));

  public async Task<bool> Handle(DeleteHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = await _hardwareRepository.GetAsync(request.Id, cancellationToken);
    if (hardware is null)
      return false;

    return await _hardwareRepository.DeleteAsync(request.Id, cancellationToken);
  }
}
