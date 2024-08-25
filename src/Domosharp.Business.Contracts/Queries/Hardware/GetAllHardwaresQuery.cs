using Domosharp.Business.Contracts.Models;
using MediatR;

namespace Domosharp.Business.Contracts.Queries.Hardware;

public record GetAllHardwaresQuery : IRequest<IEnumerable<IHardware>>
{
}
