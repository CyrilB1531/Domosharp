using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Entities;

using System.Text.Json.Nodes;

namespace Domosharp.Infrastructure.HostedServices;

internal class TasmotaDeviceService(TasmotaDevice device, IDeviceRepository deviceRepository)
{
  private Task HandleStateTopicAsync(string command, string payload, CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }

  private void SetDeviceValue(JsonObject payload, string key)
  {
    const int OffState = 0;
    const int OnState = 1;
    const int ToggleState = 2;
    JsonNode? node;
    payload.TryGetPropertyValue(key, out node);
    var value = node?.GetValue<string>();
    if (value is not null)
    {
      if (value == device.States[OffState])
        device.Value = 0;
      else if (value == device.States[OnState])
        device.Value = 100;
      else if (value == device.States[ToggleState])
        device.Value = device.Value == 0 ? 100 : 0;
    }
  }

  private bool SetShutterDeviceValue(JsonObject payload, string key)
  {
    JsonNode? shutterNode;
    payload.TryGetPropertyValue(key, out shutterNode);
    if (shutterNode is null)
      return false;

    var shutter = shutterNode.AsObject();
    shutter.TryGetPropertyValue("Position", out var positionNode);
    shutter.TryGetPropertyValue("Target", out var targetNode);
    var position = positionNode?.GetValue<int>();
    var target = targetNode?.GetValue<int>();
    if (position is null || target is null || position != target)
      return false;
    device.Value = position;
    return true;
  }
  private bool SetTemperatureDeviceValue(JsonObject payload, string key)
  {
    JsonNode? esp32Node;
    payload.TryGetPropertyValue(key, out esp32Node);
    if (esp32Node is null)
      return false;

    var esp32 = esp32Node.AsObject();
    esp32.TryGetPropertyValue("Temperature", out var temperatureNode);
    var temperature = temperatureNode?.GetValue<decimal>();
    if (temperature is null)
      return false;
    device.Value = temperature;
    return true;
  }

  private async Task ProcessStateTelemetryTopicAsync(string payload, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(payload))
      return;
    var payloadNode = JsonNode.Parse(payload)?.AsObject();
    if (payloadNode is null)
      return;

    JsonNode? node;
    payloadNode.TryGetPropertyValue("Time", out node);
    var time = node?.GetValue<DateTime>();

    switch (device.Type)
    {
      case DeviceType.LightSwitch:
        if (device.Index is null)
          SetDeviceValue(payloadNode, "POWER");
        else
          SetDeviceValue(payloadNode, $"POWER{device.Index}");
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
    device.SignalLevel = signal.GetValue<int>();
    await deviceRepository.UpdateAsync(device, cancellationToken);
  }

  private async Task ProcessSensorTelemetryTopicAsync(string payload, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(payload))
      return;
    var payloadNode = JsonNode.Parse(payload)?.AsObject();
    if (payloadNode is null)
      return;

    JsonNode? node;
    payloadNode.TryGetPropertyValue("Time", out node);
    var time = node?.GetValue<DateTime>();

    bool result;
    switch (device.Type)
    {
      case DeviceType.Blinds:
        if (device.Index is null)
          result = SetShutterDeviceValue(payloadNode, "Shutter1");
        else
          result = SetShutterDeviceValue(payloadNode, $"Shutter{device.Index}");
        break;
      case DeviceType.Sensor:
        result = SetTemperatureDeviceValue(payloadNode, "ESP32");
        break;
      default:
        return;
    }
    if (result)
      await deviceRepository.UpdateAsync(device, cancellationToken);
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
    if (command.StartsWith(device.TelemetryTopic))
    {
      await HandleTelemetryTopicAsync(command.Replace(device.TelemetryTopic, string.Empty), payload, cancellationToken);
      return true;
    }
    if (command.StartsWith(device.StateTopic))
    {
      await HandleStateTopicAsync(command.Replace(device.StateTopic, string.Empty), payload, cancellationToken);
      return true;
    }
    return false;
  }

  public TasmotaDevice Device { get { return device; } }
}
