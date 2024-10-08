﻿using Domosharp.Business.Contracts.Factories;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;
using Domosharp.Infrastructure.Factories;

using DotNetCore.CAP;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace Domosharp.Infrastructure.HostedServices;

public abstract class HardwareServiceBase : IHardwareService
{
  private bool _isStopRequested;
  protected readonly IDeviceServiceFactory DeviceServiceFactory;
  protected readonly IDeviceRepository DeviceRepository;
  protected readonly ICapPublisher CapPublisher;
  protected readonly IHardware Hardware;
  protected readonly ILogger Logger;
  protected readonly List<IDeviceService> DeviceServices;

  protected HardwareServiceBase(ICapPublisher capPublisher,
    IDeviceRepository deviceRepository,
    IHardware hardware,
    IDeviceServiceFactory deviceServiceFactory,
    ILogger logger)
  {
    DeviceRepository = deviceRepository;
    CapPublisher = capPublisher;
    Hardware = hardware;
    IsStarted = false;
    IsStopRequested = false;
    DeviceServiceFactory = deviceServiceFactory;
    DeviceServices = [];
    Logger = logger;
  }

  public abstract Task ConnectAsync(CancellationToken cancellationToken);
  public abstract Task DisconnectAsync(CancellationToken cancellationToken);

  public virtual Task<Device?> CreateDeviceAsync(Device device, CancellationToken cancellationToken = default)
  {
    return DeviceRepository.CreateAsync(device, cancellationToken);
  }

  public virtual async Task<IDeviceService?> CreateDeviceServiceAsync(Device device, CancellationToken cancellationToken = default)
  {
    var deviceService = await DeviceServiceFactory.CreateDeviceServiceAsync(device, cancellationToken);
    if (deviceService is not null)
      DeviceServices.Add(deviceService);
    return deviceService;
  }

  public virtual Task DeleteDeviceServiceAsync(Device device, CancellationToken cancellationToken = default)
  {
    var deviceService = DeviceServices.Find(a => a.Device == device);
    if(deviceService is not null)
      DeviceServices.Remove(deviceService);
    return Task.CompletedTask;
  }

  public void UpdateDevice(object? sender, DeviceEventArgs deviceEventArgs)
  {
    var device = DeviceRepository.GetAsync(deviceEventArgs.Device.Id, CancellationToken.None).GetAwaiter().GetResult();
    if (device is null)
      return;
    DeviceRepository.UpdateAsync(deviceEventArgs.Device, CancellationToken.None).GetAwaiter().GetResult();
  }

  protected ConcurrentQueue<IMessage> Messages = new();

  public virtual void EnqueueMessage(IMessage message)
  {
    Messages.Enqueue(message);
  }

  public virtual IMessage? DequeueMessage()
  {
    if (!Messages.TryDequeue(out var message))
      return null;
    return message;
  }

  public bool IsStarted { get; set; }

  public bool IsStopRequested
  {
    get
    {
      if (!IsStarted)
        return false;

      if (_isStopRequested)
        return true;

      if (IsRestartRequested)
        return true;

      return false;
    }
    set => _isStopRequested = value;
  }

  public bool IsRestartRequested { get; set; }

  public void Stop()
  {
    if (!IsStarted || IsStopRequested)
      return;
    IsStopRequested = true;
  }

  public void Restart()
  {
    Stop();
    IsRestartRequested = true;
  }

  protected virtual Task SendDataAsync(Device device, string command, int? value, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  protected virtual Task UpdateDataAsync(Device device, int? value, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public Task SendValueAsync(Device device, string command, int? value, CancellationToken cancellationToken = default)
  {
    if (!Hardware.Enabled || !device.Active)
      return Task.CompletedTask;
    EnqueueMessage(new Message(MessageType.SendValue, device, command, value));
    return Task.CompletedTask;
  }

  public Task UpdateValueAsync(Device device, int? value, CancellationToken cancellationToken = default)
  {
    if (!Hardware.Enabled || !device.Active)
      return Task.CompletedTask;
    EnqueueMessage(new Message(MessageType.UpdateValue, device, string.Empty, value));
    return Task.CompletedTask;
  }

  public async Task DoWorkAsync(CancellationToken cancellationToken = default)
  {
    if (!Hardware.Enabled)
      return;

    Hardware.UpdateDevice += UpdateDevice;

    await ConnectAsync(cancellationToken);
    IsStarted = true;
    while (!IsStopRequested)
    {
      await ProcessLoop(cancellationToken);
      Thread.Sleep(1000);
    }

    await DisconnectAsync(cancellationToken);
    IsStarted = false;

    Hardware.UpdateDevice -= UpdateDevice;
  }

  internal async Task ProcessLoop(CancellationToken cancellationToken)
  {
    var message = DequeueMessage();
    if (message is not null)
    {
      switch (message.Type)
      {
        case MessageType.UpdateValue:
          await UpdateDataAsync(message.Device, message.Value, cancellationToken);
          break;
        case MessageType.SendValue:
          await SendDataAsync(message.Device, message.Command, message.Value, cancellationToken);
          break;
      }
    }
  }

}
