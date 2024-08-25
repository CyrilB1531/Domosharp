using MediatR;

namespace Domosharp.Business.Contracts.Commands.Hardware;

public record DeleteHardwareCommand : IRequest<bool>
{
  public int Id { get; init; }
}
