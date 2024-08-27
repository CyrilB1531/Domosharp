using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Internal;
using MQTTnet.PacketDispatcher;
using MQTTnet.Packets;
using MQTTnet;
using System.Diagnostics.CodeAnalysis;

namespace Domosharp.Infrastructure.Tests.Fakes;

[ExcludeFromCodeCoverage]
internal sealed class MqttClientTest : IMqttClient
{
  public MqttClientTest()
  {
    Options = new();
  }

  readonly MqttPacketIdentifierProvider _packetIdentifierProvider = new();
  readonly MqttPacketDispatcher _packetDispatcher = new();

  readonly AsyncEvent<MqttClientConnectedEventArgs> _connectedEvent = new();
  readonly AsyncEvent<MqttClientConnectingEventArgs> _connectingEvent = new();
  readonly AsyncEvent<MqttClientDisconnectedEventArgs> _disconnectedEvent = new();
  readonly AsyncEvent<MqttApplicationMessageReceivedEventArgs> _applicationMessageReceivedEvent = new();
  readonly AsyncEvent<InspectMqttPacketEventArgs> _inspectPacketEvent = new();

  volatile int _connectionStatus;

  public event Func<MqttClientConnectedEventArgs, Task> ConnectedAsync
  {
    add => _connectedEvent.AddHandler(value);
    remove => _connectedEvent.RemoveHandler(value);
  }

  public event Func<MqttClientConnectingEventArgs, Task> ConnectingAsync
  {
    add => _connectingEvent.AddHandler(value);
    remove => _connectingEvent.RemoveHandler(value);
  }

  public event Func<MqttClientDisconnectedEventArgs, Task> DisconnectedAsync
  {
    add => _disconnectedEvent.AddHandler(value);
    remove => _disconnectedEvent.RemoveHandler(value);
  }

  public event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceivedAsync
  {
    add => _applicationMessageReceivedEvent.AddHandler(value);
    remove => _applicationMessageReceivedEvent.RemoveHandler(value);
  }

  public event Func<InspectMqttPacketEventArgs, Task> InspectPackage
  {
    add => _inspectPacketEvent.AddHandler(value);
    remove => _inspectPacketEvent.RemoveHandler(value);
  }

  public bool IsConnected => (MqttClientConnectionStatus)_connectionStatus == MqttClientConnectionStatus.Connected;

  public MqttClientOptions Options { get; private set; }

  public async Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(options);

    if (options.ChannelOptions is null)
      throw new ArgumentException("ChannelOptions are not set.");

    ThrowIfConnected("It is not allowed to connect with a server after the connection is established.");

    ObjectDisposedException.ThrowIf(_disposed, this);

    if (CompareExchangeConnectionStatus(MqttClientConnectionStatus.Connecting, MqttClientConnectionStatus.Disconnected) != MqttClientConnectionStatus.Disconnected)
    {
      throw new InvalidOperationException("Not allowed to connect while connect/disconnect is pending.");
    }

    MqttClientConnectResult connectResult = new();


    Options = options;

    if (_connectingEvent.HasHandlers)
    {
      await _connectingEvent.InvokeAsync(new MqttClientConnectingEventArgs(options));
    }

    _packetIdentifierProvider.Reset();
    _packetDispatcher.CancelAll();

    _connectionStatus = (int)MqttClientConnectionStatus.Connected;
    CompareExchangeConnectionStatus(MqttClientConnectionStatus.Connected, MqttClientConnectionStatus.Connecting);

    if (_connectedEvent.HasHandlers)
    {
      var eventArgs = new MqttClientConnectedEventArgs(connectResult);
      await _connectedEvent.InvokeAsync(eventArgs).ConfigureAwait(false);
    }

    return connectResult;
  }

  public Task DisconnectAsync(MqttClientDisconnectOptions options, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(options);

    ObjectDisposedException.ThrowIf(_disposed, this);

    DisconnectIsPendingOrFinished();
    return Task.CompletedTask;
  }

  public Task PingAsync(CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }

  public Task SendExtendedAuthenticationExchangeDataAsync(MqttExtendedAuthenticationExchangeData data, CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }

  public Task<MqttClientSubscribeResult> SubscribeAsync(MqttClientSubscribeOptions options, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(new MqttClientSubscribeResult(_packetIdentifierProvider.GetNextPacketIdentifier(), null, "Success", null));
  }

  public Task<MqttClientUnsubscribeResult> UnsubscribeAsync(MqttClientUnsubscribeOptions options, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(new MqttClientUnsubscribeResult(_packetIdentifierProvider.GetNextPacketIdentifier(), null, "Success", null));
  }

  public int PublishCount { get; private set; }

  public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
  {
    PublishCount++;
    return Task.FromResult(new MqttClientPublishResult(_packetIdentifierProvider.GetNextPacketIdentifier(), MqttClientPublishReasonCode.Success, MqttClientPublishReasonCode.Success.ToString(), null));
  }

  void ThrowIfConnected(string message)
  {
    if (IsConnected)
    {
      throw new MqttProtocolViolationException(message);
    }
  }

  static Task AcknowledgeReceivedPublishPacket(MqttApplicationMessageReceivedEventArgs eventArgs, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public async Task<MqttApplicationMessageReceivedEventArgs> HandleReceivedApplicationMessageAsync(MqttPublishPacket publishPacket)
  {
    var applicationMessage = MqttApplicationMessageFactory.Create(publishPacket);
    var eventArgs = new MqttApplicationMessageReceivedEventArgs("ClientId", applicationMessage, publishPacket, AcknowledgeReceivedPublishPacket);

    await _applicationMessageReceivedEvent.InvokeAsync(eventArgs).ConfigureAwait(false);

    return eventArgs;
  }

  void DisconnectIsPendingOrFinished()
  {
    var connectionStatus = (MqttClientConnectionStatus)_connectionStatus;

    do
    {
      switch (connectionStatus)
      {
        case MqttClientConnectionStatus.Disconnected:
        case MqttClientConnectionStatus.Disconnecting:
          return;
        case MqttClientConnectionStatus.Connected:
        case MqttClientConnectionStatus.Connecting:
          // This will compare the _connectionStatus to old value and set it to "MqttClientConnectionStatus.Disconnecting" afterwards.
          // So the first caller will get a "false" and all subsequent ones will get "true".
          var curStatus = CompareExchangeConnectionStatus(MqttClientConnectionStatus.Disconnecting, connectionStatus);
          if (curStatus == connectionStatus)
            return;

          connectionStatus = curStatus;
          break;
      }
    } while (true);
  }

  MqttClientConnectionStatus CompareExchangeConnectionStatus(MqttClientConnectionStatus value, MqttClientConnectionStatus comparand)
  {
    return (MqttClientConnectionStatus)Interlocked.CompareExchange(ref _connectionStatus, (int)value, (int)comparand);
  }

  private bool _disposed;

  public event Func<InspectMqttPacketEventArgs, Task> InspectPacketAsync
  {
    add => _inspectPacketEvent.AddHandler(value);
    remove => _inspectPacketEvent.RemoveHandler(value);
  }

  public void Dispose()
  {
    _disposed = true;
  }
}