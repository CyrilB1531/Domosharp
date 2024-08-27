using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Hardwares;

internal record Mqtt : HardwareBase, IMqttHardware
{
  private MqttConfiguration _mqttConfiguration;
  private string? _sslCertificate;
  public Mqtt(MqttConfiguration configuration, string? sslCertificate) { 
    _mqttConfiguration = configuration;
    _sslCertificate = sslCertificate;
    Type = HardwareType.MQTT;
  }

  public MqttConfiguration MqttConfiguration { get { return _mqttConfiguration; } }
  public string? SslCertificate { get { return _sslCertificate; } }

  public override void CopyTo(ref IHardware hardware)
  {
    if (hardware.GetType() != typeof(Mqtt))
      return;

    base.CopyTo(ref hardware);
    var h = (Mqtt)hardware;

    h._mqttConfiguration = _mqttConfiguration;
    h._sslCertificate = _sslCertificate;
  }

}
