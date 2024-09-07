using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;

using MQTTnet.Extensions.ManagedClient;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Domosharp.Infrastructure.HostedServices;

internal class TasmotaDeviceService : IMqttDeviceService
{
  private readonly TasmotaDevice _device;
  private readonly IDeviceRepository _deviceRepository;

  internal TasmotaDeviceService(TasmotaDevice device, IDeviceRepository deviceRepository){
    _device = device;
    _deviceRepository = deviceRepository;
  }

  public IEnumerable<string> GetSubscriptions() => [ _device.TelemetryTopic, _device.StateTopic ];

  private async Task HandleStateTopicAsync(string command, string payload, CancellationToken cancellationToken = default)
  {
    if (command != "RESULT")
      return;
    if (string.IsNullOrEmpty(payload))
      return;

    switch (_device.Type)
    {
      case DeviceType.Blinds:
        JsonNode jsonPayload = JsonNode.Parse(payload)!;
        TasmotaShutterPayload? shutter;
        if (_device.Index is null || _device.Index <= 1)
          shutter = jsonPayload["Shutter1"]?.Deserialize<TasmotaShutterPayload>();
        else
          shutter = jsonPayload[$"Shutter{_device.Index}"]?.Deserialize<TasmotaShutterPayload>();

        if (shutter is null)
          return;
        if (shutter.Position != shutter.Target)
          return;
        if (_device.Value == shutter.Position)
          return;
        _device.Value = shutter.Position;
        await _deviceRepository.UpdateAsync(_device, cancellationToken);
        break;
      default:
        break;
    }
  }

  private void SetDeviceValue(JsonObject payload, string key)
  {
    const int OffState = 0;
    const int OnState = 1;
    const int ToggleState = 2;
    payload.TryGetPropertyValue(key, out JsonNode? node);
    var value = node?.GetValue<string>();
    if (value is not null)
    {
      if (value == _device.States[OffState])
        _device.Value = 0;
      else if (value == _device.States[OnState])
        _device.Value = 100;
      else if (value == _device.States[ToggleState])
        _device.Value = _device.Value == 0 ? 100 : 0;
    }
  }

  private bool SetShutterDeviceValue(JsonObject payload, string key)
  {
    payload.TryGetPropertyValue(key, out JsonNode? shutterNode);
    if (shutterNode is null)
      return false;

    var shutter = shutterNode.AsObject();
    shutter.TryGetPropertyValue("Position", out var positionNode);
    shutter.TryGetPropertyValue("Target", out var targetNode);
    var position = positionNode?.GetValue<int>();
    var target = targetNode?.GetValue<int>();
    if (position is null || target is null || position != target)
      return false;
    _device.Value = position;
    return true;
  }

  private bool SetTemperatureDeviceValue(JsonObject payload, string key)
  {
    payload.TryGetPropertyValue(key, out JsonNode? esp32Node);
    if (esp32Node is null)
      return false;

    var esp32 = esp32Node.AsObject();
    esp32.TryGetPropertyValue("Temperature", out var temperatureNode);
    var temperature = temperatureNode?.GetValue<decimal>();
    if (temperature is null)
      return false;
    if (_device.Value == temperature)
      return false;
    _device.Value = temperature;
    return true;
  }

  private async Task ProcessStateTelemetryTopicAsync(string payload, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(payload))
      return;
    var payloadNode = JsonNode.Parse(payload)?.AsObject();
    if (payloadNode is null)
      return;

    payloadNode.TryGetPropertyValue("Time", out JsonNode? node);
    var time = node?.GetValue<DateTime>();
    if (time is not null)
      _device.LastUpdate = time.Value;
    else
      _device.LastUpdate = DateTime.Now;

    switch (_device.Type)
    {
      case DeviceType.LightSwitch:
        if (_device.Index is null)
          SetDeviceValue(payloadNode, "POWER");
        else
          SetDeviceValue(payloadNode, $"POWER{_device.Index}");
        break;
      default:
        break;
    }

    payloadNode.TryGetPropertyValue("Wifi", out node);
    if (node is null)
      return;

    node.AsObject().TryGetPropertyValue("Signal", out var signal);
    if (signal is null)
      return;
    _device.SignalLevel = signal.GetValue<int>();
    await _deviceRepository.UpdateAsync(_device, cancellationToken);
  }

  private async Task ProcessSensorTelemetryTopicAsync(string payload, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(payload))
      return;
    var payloadNode = JsonNode.Parse(payload)?.AsObject();
    if (payloadNode is null)
      return;

    payloadNode.TryGetPropertyValue("Time", out JsonNode? node);
    var time = node?.GetValue<DateTime>();
    if (time is not null)
      _device.LastUpdate = time.Value;
    else
      _device.LastUpdate = DateTime.Now;
    bool result;
    switch (_device.Type)
    {
      case DeviceType.Blinds:
        if (_device.Index is null)
          result = SetShutterDeviceValue(payloadNode, "Shutter1");
        else
          result = SetShutterDeviceValue(payloadNode, $"Shutter{_device.Index}");
        break;
      case DeviceType.Sensor:
        result = SetTemperatureDeviceValue(payloadNode, "ESP32");
        break;
      default:
        return;
    }
    if (result)
      await _deviceRepository.UpdateAsync(_device, cancellationToken);
  }

  private async Task HandleTelemetryTopicAsync(string command, string payload, CancellationToken cancellationToken = default)
  {
    const string StateCommand = "STATE";
    const string SensorCommand = "SENSOR";

    switch (command)
    {
      case StateCommand:
        await ProcessStateTelemetryTopicAsync(payload, cancellationToken);
        break;
      case SensorCommand:
        await ProcessSensorTelemetryTopicAsync(payload, cancellationToken);
        break;
      default:
        break;
    }
  }

  public async Task<bool> HandleAsync(string command, string payload, CancellationToken cancellationToken = default)
  {
    if (command.StartsWith(_device.TelemetryTopic))
    {
      await HandleTelemetryTopicAsync(command.Replace(_device.TelemetryTopic, string.Empty), payload, cancellationToken);
      return true;
    }
    if (command.StartsWith(_device.StateTopic))
    {
      await HandleStateTopicAsync(command.Replace(_device.StateTopic, string.Empty), payload, cancellationToken);
      return true;
    }
    return false;
  }

  public Device Device { get { return _device; } }
}
