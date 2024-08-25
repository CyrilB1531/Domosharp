using MediatR;

using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Commands.Hardwares;

public record UpdateHardwareCommand : IRequest<bool>
{
  public int Id { get; init; }

  public string Name { get; init; } = string.Empty;

  public bool Enabled { get; init; }

  public LogLevel LogLevel { get; init; }

  public int Order { get; init; }

  public string? Configuration { get; init; }
}
