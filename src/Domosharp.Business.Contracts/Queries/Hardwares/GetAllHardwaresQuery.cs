using Domosharp.Business.Contracts.Models;

using MediatR;

namespace Domosharp.Business.Contracts.Queries.Hardwares;

public record GetAllHardwaresQuery : IRequest<IEnumerable<IHardware>>
{
}
