using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Hardwares;

public interface IMqttHardware : IHardware
{
  MqttConfiguration MqttConfiguration { get; }
  string? SslCertificate { get; }
}
