using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Devices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Queries.Devices;

public class GetAllDevicesQueryHandler(IDeviceRepository deviceRepository) : IRequestHandler<GetAllDevicesQuery, IEnumerable<Device>>
{
  public Task<IEnumerable<Device>> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
  {
    return deviceRepository.GetListAsync(request.HardwareId, cancellationToken);
  }
}
