using MediatR;

namespace Domosharp.Business.Contracts.Queries.Devices
{
  public record GetAllDevicesQuery() : IRequest<IEnumerable<Models.Device>>
  {
    public int HardwareId { get; init; }
  }
}
