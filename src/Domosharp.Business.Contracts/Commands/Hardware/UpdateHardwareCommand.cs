using Domosharp.Business.Contracts.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Domosharp.Business.Contracts.Commands.Hardware;

public record UpdateHardwareCommand : IRequest<bool>
{
  public int Id { get; init; }

  public string Name { get; init; } = string.Empty;

  public bool Enabled { get; init; }

  public HardwareType Type { get; init; }

  public LogLevel LogLevel { get; init; }

  public int Order { get; init; }

  public string? Configuration { get; init; }
}
