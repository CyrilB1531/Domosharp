using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
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
  IDeviceServiceFactory deviceServiceFactory,
  ILogger logger) : MqttService(capPublisher, deviceRepository, clientIn, clientOut, hardware, deviceServiceFactory, logger)
{
  private static bool DevicesAreDifferent(TasmotaDevice oldDevice, TasmotaDevice newDevice)
  {
    return oldDevice.SpecificParameters != newDevice.SpecificParameters;
  }

  public override async Task DeleteDeviceServiceAsync(Device device, CancellationToken cancellationToken = default)
  {
    if (DeviceServices.First(a => a.Device == device) is not TasmotaDeviceService oldDeviceService)
      return;
    foreach (var subscription in oldDeviceService.GetSubscriptions())
      await ClientIn.UnsubscribeAsync(subscription);
    await base.DeleteDeviceServiceAsync(device, cancellationToken);
  }

  public override async Task<IDeviceService?> CreateDeviceServiceAsync(Device device, CancellationToken cancellationToken = default)
  {
    var service = await base.CreateDeviceServiceAsync(device, cancellationToken);
    if (service is not TasmotaDeviceService tasmotaService)
      return null;

    foreach (var subscription in tasmotaService.GetSubscriptions())
      await ClientIn.SubscribeAsync(subscription);

    return service;
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
        await DeleteDeviceServiceAsync(oldDevice, cancellationToken);
        await CreateDeviceServiceAsync(newDevice, cancellationToken);
        return;
      }
    }
    // Insert
    await DeviceRepository.CreateAsync(newDevice, cancellationToken);
    await CreateDeviceServiceAsync(newDevice, cancellationToken);
  }

  private async Task AddDeviceServicesAsync(List<TasmotaDevice?> devices, CancellationToken cancellationToken)
  {
    if (DeviceServices.Count == 0)
    {
      foreach (var device in devices.Where(a => a is not null))
        await CreateDeviceServiceAsync(device!, cancellationToken);
    }
  }

  protected async Task<bool> ProcessTasmotaDiscoveryConfigTopicMessage(List<TasmotaDevice?> devices, string payload, CancellationToken cancellationToken)
  {
    TasmotaDiscoveryPayload discoveryPayload;
    try
    {
      if (string.IsNullOrEmpty(payload))
        return false;
      discoveryPayload = JsonSerializer.Deserialize<TasmotaDiscoveryPayload>(payload, JsonExtensions.FullObjectOnDeserializing)!;
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
      TasmotaDevice newDevice = new(Hardware.Id, discoveryPayload, index: count > 1 ? (i + indexIncrementer) / indexIncrementer : null) { Active = true };
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

    var newDevice = new TasmotaDevice(Hardware.Id, JsonSerializer.Deserialize<TasmotaDiscoveryPayload>(firstDevice.SpecificParameters!, JsonExtensions.FullObjectOnDeserializing)!, DeviceType.Sensor) { Active = true };
    var json = JsonNode.Parse(payload);
    if (json is null)
      return false;

    var time = json["Time"]?.AsValue()?.GetValue<DateTime>();
    if (time is not null)
      newDevice.LastUpdate = time.Value;
    devices.Add(newDevice);
    await DeviceRepository.CreateAsync(newDevice, cancellationToken);
    if((await CreateDeviceServiceAsync(newDevice, cancellationToken)) is not TasmotaDeviceService service)
        return false;

    await service!.HandleAsync(newDevice.TelemetryTopic + "SENSOR", payload, cancellationToken);
    return true;
  }

  private static JsonNode? GetTasmotaDiscoverySensorsTopicJson(string payload)
  {
    var json = JsonNode.Parse(payload);
    if (json is null)
      return null;

    if (json["sn"] is not null)
      json = json["sn"]!;
    return json;
  }

  private async Task<bool> ProcessShutterResultFromDiscoverySensorTopic(JsonNode json, IEnumerable<TasmotaDevice> matchingDevices, int index, CancellationToken cancellationToken)
  {
    var shutterDevice = matchingDevices.FirstOrDefault(a => a.Type == DeviceType.Blinds && (a.Index == index || (a.Index is null && index == 1)));
    if (shutterDevice is null)
      return false;
    var deviceService = DeviceServices.Find(a => a.Device.Id == shutterDevice.Id);
    deviceService ??= await CreateDeviceServiceAsync(shutterDevice, cancellationToken);

    await ((TasmotaDeviceService)deviceService!).HandleAsync(shutterDevice.StateTopic + "RESULT", "{\"Shutter" + index + "\":" + JsonSerializer.Serialize(json["Shutter" + index.ToString()]) + "}", cancellationToken);
    return true;
  }

  protected async Task<bool> ProcessTasmotaDiscoverySensorsTopicMessage(List<TasmotaDevice?> devices, IEnumerable<TasmotaDevice> matchingDevices, string payload, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(payload))
      return false;
    var json = GetTasmotaDiscoverySensorsTopicJson(payload);
    if (json is null)
      return false;

    if (json["ESP32"] is not null)
      await ProcessTemperatureDeviceMessage(devices, matchingDevices.FirstOrDefault(), payload, cancellationToken);

    for (var index = 1; index < 33; index++)
    {
      if (json["Shutter" + index.ToString()] is not null)
      {
        if (!await ProcessShutterResultFromDiscoverySensorTopic(json, matchingDevices, index, cancellationToken))
          return false;
      }
      else if (index > 1)
        return index > 1;
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
      var macAddress = topic.Replace(SensorsTopic, string.Empty).Replace("/", string.Empty);
      return ProcessTasmotaDiscoverySensorsTopicMessage(devices, devices.Where(a => a is not null && a.Type != DeviceType.Sensor && a.MacAddress == macAddress).Select(a => a!), payload, cancellationToken);
    }
    return Task.FromResult(true);
  }

  protected override async Task<bool> ProcessMessageReceivedAsync(string topic, string payload, CancellationToken cancellationToken = default)
  {
    var devices = (await DeviceRepository.GetListAsync(Hardware.Id, cancellationToken)).Select(a => a.MapToTasmotaDevice()).Where(a => a is not null && a.Active).ToList();
    await AddDeviceServicesAsync(devices, cancellationToken);

    var tasmotaDiscoveryTopic = ((IMqttHardware)Hardware).MqttConfiguration.SubscriptionsIn[0];
    if (topic.StartsWith(tasmotaDiscoveryTopic))
      return await ProcessMessageInSubscribedTopic(devices, topic.Replace(tasmotaDiscoveryTopic, string.Empty), payload, cancellationToken);

    var result = false;
    foreach (var device in DeviceServices)
      result |= await ((TasmotaDeviceService)device).HandleAsync(topic, payload, cancellationToken);

    return result;
  }
}
