using Domosharp.Business.Contracts.Models;

using MediatR;

namespace Domosharp.Business.Contracts.Queries.Devices
{
  public record GetAllDevicesQuery() : IRequest<IEnumerable<Device>>
  {
    public int HardwareId { get; init; }
  }
}
