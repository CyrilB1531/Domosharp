using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Hardwares;

internal record MqttTasmota : Mqtt
{
  public MqttTasmota(MqttConfiguration configuration, string? sslCertificate) : base(configuration, sslCertificate)
  {
  }
}
