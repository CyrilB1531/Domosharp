﻿using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Hardwares;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging;

using MQTTnet.Extensions.ManagedClient;

namespace Domosharp.Infrastructure.HostedServices;

public class HardwareServiceFactory(
  ICapPublisher capPublisher,
    IDeviceRepository deviceRepository,
    IManagedMqttClient clientIn,
    IManagedMqttClient clientOut,
    ILogger<HardwareServiceFactory> logger) : IHardwareServiceFactory
{
  public IHardwareService CreateFromHardware(IHardware hardware)
  {
    return hardware.Type switch
    {
      HardwareType.MQTTTasmota => new MqttTasmotaService(capPublisher, deviceRepository, clientIn, clientOut, (MqttTasmota)hardware, logger),
      HardwareType.MQTT => new MqttService(capPublisher, deviceRepository, clientIn, clientOut, (IMqttHardware)hardware, logger),
      HardwareType.Dummy => new DummyService(capPublisher, deviceRepository, hardware),
      _ => throw new NotImplementedException(),
    };
  }
}
