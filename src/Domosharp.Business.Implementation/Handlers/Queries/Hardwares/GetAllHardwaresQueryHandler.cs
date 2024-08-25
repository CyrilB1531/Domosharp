using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Queries.Hardwares;
using Domosharp.Business.Contracts.Repositories;
using MediatR;

namespace Domosharp.Business.Implementation.Handlers.Queries.Hardwares;

public class GetAllHardwaresQueryHandler(IHardwareRepository hardwareRepository) : IRequestHandler<GetAllHardwaresQuery, IEnumerable<IHardware>>
{
  public Task<IEnumerable<IHardware>> Handle(GetAllHardwaresQuery request, CancellationToken cancellationToken)
  {
    return hardwareRepository.GetListAsync(cancellationToken);
  }
}
