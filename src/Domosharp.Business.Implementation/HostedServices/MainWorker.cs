using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using System.Collections.Concurrent;

using MessageType = Domosharp.Business.Contracts.Models.MessageType;

namespace Domosharp.Business.Implementation.HostedServices;

public class MainWorker(IHardwareWorker hardwareWorker, IHardwareRepository hardwareRepository) : IMainWorker
{
  public ConcurrentQueue<IMessage> Messages { get; } = new ConcurrentQueue<IMessage>();

  private List<IHardware>? _hardwares;

  private Thread? _workerThread = null;
  private bool _stopThread = false;
  private static readonly object _locker = new();

  public Task StartAsync(CancellationToken cancellationToken)
  {
    if (_workerThread is null)
    {
      _workerThread = new Thread(new ThreadStart(DoWork))
      {
        IsBackground = true,
        Name = "MainWorker"
      };
      _workerThread.Start();
    }
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    if (_workerThread is not null)
    {
      _stopThread = true;
      _workerThread.Join();
      _workerThread = null;
    }
    return Task.CompletedTask;
  }

  public void AddHardware(IHardware hardware)
  {
    lock (_locker)
      _hardwares?.Add(hardware);
  }

  public void UpdateHardware(IHardware hardware)
  {
    lock (_locker)
    {
      if (_hardwares is not null && !_hardwares.Exists(a => a is not null && a.Id == hardware.Id))
        return;
      var h = _hardwares?.First(a => a is not null && a.Id == hardware.Id);
      if (h is null)
        return;
      hardware.CopyTo(ref h);
    }
  }

  public void DeleteHardware(int hardwareId)
  {
    lock (_locker)
    {
      if (_hardwares is null || !_hardwares.Exists(a => a is not null && a.Id == hardwareId))
        return;
      _hardwares.Remove(_hardwares.First(a => a is not null && a.Id == hardwareId));
    }
  }

  public void DoWork()
  {
    _hardwares = hardwareRepository?.GetListAsync(true).GetAwaiter().GetResult().ToList();
    if (_hardwares is null)
      return;

    while (!_stopThread)
    {
      lock (_locker)
      {
        hardwareWorker.DoWorkAsync(_hardwares, CancellationToken.None).GetAwaiter().GetResult();
      }
      if (Messages.TryDequeue(out var message))
      {
        switch (message.Type)
        {
          case MessageType.UpdateValue:
            hardwareWorker.UpdateValueAsync(message.Device, message.Value, CancellationToken.None).GetAwaiter().GetResult();
            break;
          case MessageType.SendValue:
            hardwareWorker.SendValueAsync(message.Device, message.Command, message.Value, CancellationToken.None).GetAwaiter().GetResult();
            break;
        }
      }
      Thread.Sleep(1000);
    }
    lock (_locker)
      hardwareWorker.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
  }
}
