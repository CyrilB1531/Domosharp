using MediatR;

namespace Domosharp.Business.Contracts.Commands.Hardwares;

public record DeleteHardwareCommand : IRequest<bool>
{
  public int Id { get; init; }
}
