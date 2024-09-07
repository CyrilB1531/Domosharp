using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.HostedServices;

public interface IHardwareService
{
  Task DoWorkAsync(CancellationToken cancellationToken = default);

  Task ConnectAsync(CancellationToken cancellationToken);
  Task DisconnectAsync(CancellationToken cancellationToken);

  Task UpdateValueAsync(Device device, int? value, CancellationToken cancellationToken = default);
  Task SendValueAsync(Device device, string command, int? value, CancellationToken cancellationToken = default);

  void EnqueueMessage(IMessage message);

  IMessage? DequeueMessage();

  bool IsStarted { get; set; }

  bool IsStopRequested { get; set; }

  bool IsRestartRequested { get; set; }

  Task<Device?> CreateDeviceAsync(Device device, CancellationToken cancellationToken = default);

  void UpdateDevice(object? sender, DeviceEventArgs deviceEventArgs);

  Task<IDeviceService?> CreateDeviceServiceAsync(Device device, CancellationToken cancellationToken = default);
  Task DeleteDeviceServiceAsync(Device device, CancellationToken cancellationToken = default);

  void Stop();

  void Restart();
}
