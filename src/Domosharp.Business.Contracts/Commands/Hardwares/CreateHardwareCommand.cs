using Domosharp.Business.Contracts.Models;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Commands.Hardwares;

public record CreateHardwareCommand : IRequest
{
  public string Name { get; init; } = string.Empty;

  public bool Enabled { get; init; }

  public HardwareType Type { get; init; }

  public LogLevel LogLevel { get; init; }

  public int Order { get; init; }

  public string? Configuration { get; init; }
}
