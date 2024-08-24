using Domosharp.Business.Contracts.Commands.Hardware;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardware;

public class CreateHardwareCommandHandler(IHardwareRepository hardwareRepository) : IRequestHandler<CreateHardwareCommand>
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
  }
}
