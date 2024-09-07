using Domosharp.Business.Contracts.Models;
using Domosharp.Business.Contracts.Repositories;

using DotNetCore.CAP;

using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;

using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Microsoft.Extensions.Logging;
using Domosharp.Business.Contracts.Factories;

namespace Domosharp.Infrastructure.HostedServices;

internal class MqttService : HardwareServiceBase
{
  protected readonly IManagedMqttClient ClientIn;
  protected readonly IManagedMqttClient ClientOut;
  private readonly MqttConfiguration _mqttConfiguration;
  private readonly string? _sslCertificate;

  public MqttService(
    ICapPublisher capPublisher,
    IDeviceRepository deviceRepository,
    IManagedMqttClient clientIn,
    IManagedMqttClient clientOut,
    IMqttHardware hardware,
    IDeviceServiceFactory deviceServiceFactory,
    ILogger logger) : base(capPublisher, deviceRepository, hardware, deviceServiceFactory, logger)
  {
    if (!(hardware.Type is HardwareType.MQTT or HardwareType.MQTTTasmota))
      throw new ArgumentException("Hardware is not Mqtt type", nameof(hardware));

    _sslCertificate = hardware.SslCertificate;
    _mqttConfiguration = hardware.MqttConfiguration;
    ClientIn = clientIn ?? throw new ArgumentNullException(nameof(clientIn));
    ClientOut = clientOut ?? throw new ArgumentNullException(nameof(clientOut));
  }

  public string[] SubscriptionsIn { get => _mqttConfiguration.SubscriptionsIn; }
  public string[] SubscriptionsOut { get => _mqttConfiguration.SubscriptionsOut; }

  public override async Task ConnectAsync(CancellationToken cancellationToken)
  {
    await ConnectClientAsync(ClientIn, false, SubscriptionsIn);
    if (SubscriptionsOut.Length != 0)
      await ConnectClientAsync(ClientOut, true, SubscriptionsOut);
  }

  public override async Task DisconnectAsync(CancellationToken cancellationToken)
  {
    ClientIn.ApplicationMessageReceivedAsync -= Client_ApplicationMessageReceivedAsync;
    ClientIn.ConnectingFailedAsync -= Client_ConnectingFailedAsync;
    await ClientIn.StopAsync();

    ClientOut.ConnectingFailedAsync -= Client_ConnectingFailedAsync;
    await ClientOut.StopAsync();
  }

  protected override async Task SendDataAsync(Device device, string command, int? value, CancellationToken cancellationToken)
  {
    foreach (var subscriptionOut in _mqttConfiguration.SubscriptionsOut)
      await ClientOut.InternalClient.PublishAsync(new MqttApplicationMessageBuilder()
              .WithTopic(subscriptionOut + "/" + command)
              .WithPayload($"{value}")
              .WithContentType("string")
              .Build(),
              cancellationToken);
  }

  protected override Task UpdateDataAsync(Device device, int? value, CancellationToken cancellationToken)
  {
    if (value is null)
      return Task.CompletedTask;

    device.Value = value;
    return Task.CompletedTask;
  }

  private async Task ConnectClientAsync(IManagedMqttClient client, bool outOne, params string[] subscriptions)
  {
    MqttClientOptionsBuilder clientOptionBuilder = new MqttClientOptionsBuilder()
        .WithTcpServer(_mqttConfiguration.Address, _mqttConfiguration.Port)
        .WithCleanStart(false)
        .WithCleanSession(false)
        .WithClientId("EMQX=" + Guid.NewGuid().ToString());

    if (!string.IsNullOrWhiteSpace(_mqttConfiguration.UserName))
      clientOptionBuilder.WithCredentials(_mqttConfiguration.UserName, _mqttConfiguration.Password);
    if (_mqttConfiguration.UseTLS)
    {
      clientOptionBuilder.WithTlsOptions(o =>
      {
        o.WithSslProtocols(SslProtocols.Tls12);
        o.WithIgnoreCertificateChainErrors();
        o.WithIgnoreCertificateRevocationErrors();
        o.WithCertificateValidationHandler(_ => true);
        o.WithAllowUntrustedCertificates();
        if (!string.IsNullOrWhiteSpace(_sslCertificate))
          o.WithCertificateSelectionHandler((MqttClientCertificateSelectionEventArgs) => new X509Certificate(X509Certificate2.CreateFromPemFile(_sslCertificate).RawData));
      });
    }

    var options = clientOptionBuilder.Build();
    if (!outOne)
    {
      client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
    }
    client.ConnectingFailedAsync += Client_ConnectingFailedAsync;

    await client.StartAsync(new ManagedMqttClientOptionsBuilder().WithClientOptions(options).Build());

    var mqttTopicFilters = new List<MqttTopicFilter>();
    foreach (var subscription in subscriptions)
    {
      mqttTopicFilters.Add(new MqttTopicFilterBuilder().WithTopic(subscription + "/#").Build());
    }
    await client.SubscribeAsync(mqttTopicFilters);
  }

  private static Task Client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
  {
    return Task.CompletedTask;
  }

  private async Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
  {
    var topic = arg.ApplicationMessage.Topic;
    var payload = arg.ApplicationMessage.ConvertPayloadToString();
    var result = await ProcessMessageReceivedAsync(topic, payload, CancellationToken.None);
    if (result)
    {
      arg.IsHandled = true;
      await arg.AcknowledgeAsync(CancellationToken.None);
    }
  }

  protected virtual Task<bool> ProcessMessageReceivedAsync(string topic, string payload, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(false);
  }
}
