using MediatR;

namespace Domosharp.Business.Contracts.Queries.Device
{
  public record GetAllDevicesQuery() : IRequest<IEnumerable<Models.Device>>
  {
    public int HardwareId { get; init; }
  }
}
