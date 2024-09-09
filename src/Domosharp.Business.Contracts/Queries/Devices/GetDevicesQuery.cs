using Domosharp.Business.Contracts.Models;

using MediatR;

namespace Domosharp.Business.Contracts.Queries.Devices;

public class GetDevicesQuery : IRequest<IEnumerable<Device>>
{
  public bool OnlyFavorites {  get; set; }
  public bool OnlyActives {  get; set; }
}
