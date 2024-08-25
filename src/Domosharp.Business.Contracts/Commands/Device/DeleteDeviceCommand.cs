using MediatR;

namespace Domosharp.Business.Contracts.Commands.Device;

public record DeleteDeviceCommand : IRequest<bool>
{
  public int Id { get; init; }
}
