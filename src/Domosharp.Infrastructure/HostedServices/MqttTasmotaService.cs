using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Mappers;

using DotNetCore.CAP;

using MQTTnet.Extensions.ManagedClient;

using Newtonsoft.Json;

namespace Domosharp.Infrastructure.HostedServices;

internal class MqttTasmotaService(
  ICapPublisher capPublisher,
  IDeviceRepository deviceRepository,
  IManagedMqttClient clientIn,
  IManagedMqttClient clientOut,
  IMqttHardware hardware) : MqttService(capPublisher, deviceRepository, clientIn, clientOut, hardware)
{
  private readonly List<TasmotaDeviceService> _deviceServices = [];

  private static bool DevicesAreDifferent(TasmotaDevice oldDevice, TasmotaDevice newDevice)
  {
    if (oldDevice.SpecificParameters != newDevice.SpecificParameters)
      return true;
    if (oldDevice.SignalLevel != newDevice.SignalLevel)
      return true;
    if (oldDevice.BatteryLevel != newDevice.BatteryLevel)
      return true;
    if (oldDevice.CommandTopic != newDevice.CommandTopic)
      return true;
    if (oldDevice.StateTopic != newDevice.StateTopic)
      return true;
    if (oldDevice.TelemetryTopic != newDevice.TelemetryTopic)
      return true;
    if (oldDevice.Type == newDevice.Type)
      return true;
    return false;
  }
  private async Task ProcessOneSubscriptionDevice(List<TasmotaDevice?> devices, TasmotaDevice newDevice, TasmotaDiscoveryPayload discoveryPayload, CancellationToken cancellationToken)
  {
    if (devices.Count != 0)
    {
      var oldDevice = devices.Find(a => a is not null && a.DeviceId == discoveryPayload.FullMacAsDeviceId && a.Name == newDevice.Name);
      if (oldDevice is not null)
      {
        if (!oldDevice.Active)
          return;

        // Update
        var hasChanges = DevicesAreDifferent(oldDevice, newDevice);

        if (!hasChanges)
          return;
        await DeviceRepository.UpdateAsync(newDevice, cancellationToken);
        _deviceServices.Remove(_deviceServices.First(a => a.Device == oldDevice));
        _deviceServices.Add(new TasmotaDeviceService(newDevice, DeviceRepository));
      }
      else
      {
        // Insert
        await DeviceRepository.CreateAsync(newDevice, cancellationToken);
        _deviceServices.Add(new TasmotaDeviceService(newDevice, DeviceRepository));
      }
      return;
    }
    // Insert
    await DeviceRepository.CreateAsync(newDevice, cancellationToken);
    _deviceServices.Add(new TasmotaDeviceService(newDevice, DeviceRepository));
  }

  private void AddDeviceServices(List<TasmotaDevice?> devices)
  {
    if (_deviceServices.Count == 0)
    {
      foreach (var device in devices.Where(a => a is not null))
        _deviceServices.Add(new TasmotaDeviceService(device!, DeviceRepository));
    }
  }

  protected override async Task<bool> ProcessMessageReceivedAsync(string topic, string payload, CancellationToken cancellationToken = default)
  {
    var devices = (await DeviceRepository.GetListAsync(Hardware.Id, cancellationToken)).Select(a => a.MapToTasmotaDevice()).Where(a => a is not null && a.Active).ToList();
    AddDeviceServices(devices);

    if (topic.StartsWith(((IMqttHardware)Hardware).MqttConfiguration.SubscriptionsIn[0]))
    {
      var discoveryPayload = JsonConvert.DeserializeObject<TasmotaDiscoveryPayload>(payload);
      if (discoveryPayload is null)
        return false;

      var count = discoveryPayload.Relays.Count(b => b != RelayType.None);
      var indexIncrementer = 1;
      if (discoveryPayload.Relays[0] == RelayType.Shutter)
      {
        count /= 2;
        indexIncrementer = 2;
      }

      for (var i = 0; i < count * indexIncrementer; i += indexIncrementer)
      {
        TasmotaDevice newDevice = new(Hardware, discoveryPayload, count > 1 ? (i + indexIncrementer) / indexIncrementer : null);
        await ProcessOneSubscriptionDevice(devices, newDevice, discoveryPayload, cancellationToken);
      }
      return true;
    }

    foreach (var device in _deviceServices)
    {
      if (await device.HandleAsync(topic, payload, cancellationToken))
        return true;
    }
    return false;
  }
}
