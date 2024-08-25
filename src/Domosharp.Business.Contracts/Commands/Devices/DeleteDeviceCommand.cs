using MediatR;

namespace Domosharp.Business.Contracts.Commands.Devices;

public record DeleteDeviceCommand : IRequest<bool>
{
  public int Id { get; init; }
}
