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

  void CreateDevice(object? sender, DeviceEventArgs deviceEventArgs);
  void UpdateDevice(object? sender, DeviceEventArgs deviceEventArgs);

  void Stop();

  void Restart();
}
