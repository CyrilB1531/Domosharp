using Domosharp.Business.Contracts.Queries.Device;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Queries.Device;

public class GetAllDevicesQueryHandler(IDeviceRepository deviceRepository) : IRequestHandler<GetAllDevicesQuery, IEnumerable<Contracts.Models.Device>>
{
  public Task<IEnumerable<Contracts.Models.Device>> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
  {
    return deviceRepository.GetListAsync(request.HardwareId, cancellationToken);
  }
}
