using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;

using System.Runtime.CompilerServices;

using Message = Domosharp.Business.Contracts.Models.Message;
using MessageType = Domosharp.Business.Contracts.Models.MessageType;

[assembly:InternalsVisibleTo("Domosharp.Domain.Tests")]
namespace Domosharp.Business.Implementation.HostedServices;

public class HardwareWorker(IHardwareServiceFactory hardwareServiceFactory) : IHardwareWorker
{
  private readonly List<Thread> _threads = [];
  private readonly Dictionary<string, IHardwareService> _services = [];

  public Task DoWorkAsync(IEnumerable<IHardware?> hardwares, CancellationToken cancellationToken = default)
  {
    foreach (var hardware in hardwares)
    {
      if (hardware is null || !hardware.Enabled)
        continue;

      if (_threads.Exists(a => a.Name == hardware.Name))
        return Task.CompletedTask;

      var service = hardwareServiceFactory.CreateFromHardware(hardware);
      var thread = new Thread(new ParameterizedThreadStart(DoWork))
      {
        Name = hardware.Name,
        IsBackground = true
      };
      thread.Start(service);
      _threads.Add(thread);
      _services[hardware.Name] = service;
    }

    return Task.CompletedTask;
  }

  public Task SendValueAsync(Device device, string command, int? value, CancellationToken cancellationToken = default)
  {
    if (device.Hardware is null)
      throw new ArgumentException("Hardware not found", nameof(device));

    if (!device.Hardware.Enabled || !device.Active)
      return Task.CompletedTask;

    if (!_services.TryGetValue(device.Hardware.Name, out var queue))
      return Task.CompletedTask;

    queue.EnqueueMessage(new Message(MessageType.SendValue, device, command, value));
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken = default)
  {
    foreach (var service in _services.Select(a => a.Value).Where(a => a.IsStarted))
      service.Stop();

    foreach (var thread in _threads)
      thread.Join();

    return Task.CompletedTask;
  }

  public Task UpdateValueAsync(Device device, int? value, CancellationToken cancellationToken = default)
  {
    if (device.Hardware is null)
      throw new ArgumentException("Hardware not found", nameof(device));
    if (!device.Hardware.Enabled || !device.Active)
      return Task.CompletedTask;

    if (!_services.TryGetValue(device.Hardware.Name, out var queue))
      return Task.CompletedTask;

    queue.EnqueueMessage(new Message(MessageType.UpdateValue, device, string.Empty, value));
    return Task.CompletedTask;
  }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
  internal void DoWork(object? obj)
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
  {
    ArgumentNullException.ThrowIfNull(obj);

    if (obj is not IHardwareService hardware)
      throw new ArgumentException("Parameter is not an hardware service", nameof(obj));

    hardware.DoWorkAsync(CancellationToken.None).GetAwaiter().GetResult();
  }
}
