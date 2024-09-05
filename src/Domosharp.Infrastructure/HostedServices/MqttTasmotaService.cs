using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Mappers;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging;

using MQTTnet.Extensions.ManagedClient;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Domosharp.Infrastructure.HostedServices;

internal class MqttTasmotaService(
  ICapPublisher capPublisher,
  IDeviceRepository deviceRepository,
  IManagedMqttClient clientIn,
  IManagedMqttClient clientOut,
  IMqttHardware hardware,
  ILogger logger) : MqttService(capPublisher, deviceRepository, clientIn, clientOut, hardware, logger)
{
  private readonly List<TasmotaDeviceService> _deviceServices = [];

  private static bool DevicesAreDifferent(TasmotaDevice oldDevice, TasmotaDevice newDevice)
  {
    return oldDevice.SpecificParameters != newDevice.SpecificParameters;
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

  protected async Task<bool> ProcessTasmotaDiscoveryConfigTopicMessage(List<TasmotaDevice?> devices, string payload, CancellationToken cancellationToken)
  {
    TasmotaDiscoveryPayload discoveryPayload;
    try
    {

      discoveryPayload = JsonSerializer.Deserialize<TasmotaDiscoveryPayload>(payload, JsonExtensions.FullObjectOnDeserializing)!;
      if (discoveryPayload is null)
        return false;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "An error occurs");
      return false;
    }

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

  protected async Task<bool> ProcessTemperatureDeviceMessage(List<TasmotaDevice?> devices, TasmotaDevice? firstDevice, string payload, CancellationToken cancellationToken)
  {
    if (firstDevice is null)
      return false;

    if (devices.Exists(a => a is not null && a.Type == DeviceType.Sensor && a.MacAddress == firstDevice.MacAddress))
      return true;
    var newDevice = new TasmotaDevice(Hardware, JsonSerializer.Deserialize<TasmotaDiscoveryPayload>(firstDevice.SpecificParameters!, JsonExtensions.FullObjectOnDeserializing)!) { Type = DeviceType.Sensor };
    var json = JsonNode.Parse(payload);
    if (json is null)
      return false;

    var time = json["Time"]?.AsValue()?.GetValue<DateTime>();
    if(time is not null)
      newDevice.LastUpdate = time.Value;
    devices.Add(newDevice);
    await DeviceRepository.CreateAsync(newDevice, cancellationToken);
    var service = new TasmotaDeviceService(newDevice, DeviceRepository);
    _deviceServices.Add(service);
    await service.HandleAsync(newDevice.TelemetryTopic + "SENSOR", payload, cancellationToken);
    return true;
  }

  protected async Task<bool> ProcessTasmotaDiscoverySensorsTopicMessage(List<TasmotaDevice?> devices, IEnumerable<TasmotaDevice> matchingDevices, string payload, CancellationToken cancellationToken)
  {
    if(string.IsNullOrEmpty(payload)) 
      return false;
    var json = JsonNode.Parse(payload);
    if (json is null)
      return false;

    if (json["ESP32"] is not null)
      await ProcessTemperatureDeviceMessage(devices, matchingDevices.FirstOrDefault(), payload, cancellationToken);

    for (var index = 1; index < 33; index++)
    {
      if (json["Shutter" + index.ToString()] is not null)
      {
        var shutterDevice = matchingDevices.FirstOrDefault(a=> a.Type == DeviceType.Blinds && (a.Index==index || (a.Index is null && index == 1)));
        if(shutterDevice is null)
          return false;
        await _deviceServices.First(a=>a.Device == shutterDevice).HandleAsync(shutterDevice.StateTopic + "RESULT", "{\"Shutter"+index+"\":"+ JsonSerializer.Serialize(json["Shutter"+index.ToString()]) + "}", cancellationToken);
      }
      else if(index>1)
        return true;
      else
        return false;
    }
    return true;
  }

  protected Task<bool> ProcessMessageInSubscribedTopic(List<TasmotaDevice?> devices, string topic, string payload, CancellationToken cancellationToken)
  {
    const string ConfigTopic = "/config";
    const string SensorsTopic = "/sensors";
    if (topic.EndsWith(ConfigTopic))
      return ProcessTasmotaDiscoveryConfigTopicMessage(devices, payload, cancellationToken);

    if (topic.EndsWith(SensorsTopic))
    {
      var macAddress = topic.Replace(SensorsTopic, string.Empty).Replace("/",string.Empty);
      return ProcessTasmotaDiscoverySensorsTopicMessage(devices, devices.Where(a => a is not null && a.Type != DeviceType.Sensor && a.MacAddress == macAddress).Select(a=> a!), payload, cancellationToken);
    }
    return Task.FromResult(true);
  }

  protected override async Task<bool> ProcessMessageReceivedAsync(string topic, string payload, CancellationToken cancellationToken = default)
  {
    var devices = (await DeviceRepository.GetListAsync(Hardware.Id, cancellationToken)).Select(a => a.MapToTasmotaDevice()).Where(a => a is not null && a.Active).ToList();
    AddDeviceServices(devices);

    var tasmotaDiscoveryTopic = ((IMqttHardware)Hardware).MqttConfiguration.SubscriptionsIn[0];
    if (topic.StartsWith(tasmotaDiscoveryTopic))
      return await ProcessMessageInSubscribedTopic(devices, topic.Replace(tasmotaDiscoveryTopic, string.Empty), payload, cancellationToken);

    foreach (var device in _deviceServices)
      if (await device.HandleAsync(topic, payload, cancellationToken))
        return true;

    return false;
  }
}
