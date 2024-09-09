using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Devices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Queries.Devices;

public class GetDevicesQueryHandler(IDeviceRepository deviceRepository) : IRequestHandler<GetDevicesQuery, IEnumerable<Device>>
{
  public Task<IEnumerable<Device>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
  {
    return deviceRepository.GetListAsync(request.OnlyActives, request.OnlyFavorites, cancellationToken);
  }
}
