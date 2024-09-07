using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Factories;
using Domosharp.Infrastructure.Entities;
using Domosharp.Infrastructure.Hardwares;
using Domosharp.Infrastructure.Mappers;
using Domosharp.Infrastructure.Repositories;

using Microsoft.Extensions.Logging;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Domosharp.Infrastructure.Factories;

internal class HardwareFactory(
  IMqttEntityRepository mqttRepository,
  IDomosharpConfiguration configuration) : IHardwareFactory, IHardwareInfrastructureFactory
{
  private const string HardwareConfigurationNotFound = "Hardware configuration not found";
  private const string PortOutOfRange = "Port must be between 1 and 65535";
  private const string ConfigurationIsNull = "Configuration is null";

  private static string? GetMqttPassword(string? password, IDomosharpConfiguration configuration)
  {
    if (string.IsNullOrWhiteSpace(password))
      return null;

    using var memoryStream = new MemoryStream();
    Aes aes = Aes.Create();
    var bytes = Convert.FromBase64String(password);
    using var decStream = new CryptoStream(memoryStream, aes.CreateDecryptor(configuration.Aes.KeyBytes(), configuration.Aes.IVBytes()), CryptoStreamMode.Write);
    decStream.Write(bytes, 0, bytes.Length);
    decStream.Close();
    return Encoding.UTF8.GetString(memoryStream.ToArray());
  }

  private static MqttConfiguration GetMqttConfiguration(string? request)
  {
    if (string.IsNullOrEmpty(request))
      throw new ArgumentException(HardwareConfigurationNotFound, nameof(request));
    var mqttConfiguration = JsonSerializer.Deserialize<MqttConfiguration>(request) ?? throw new ArgumentException("Hardware configuration not found", nameof(request));
    if (mqttConfiguration.Port > 65535 || mqttConfiguration.Port <= 0)
      throw new ArgumentOutOfRangeException(nameof(request), PortOutOfRange);
    return mqttConfiguration;
  }

  public async Task<Business.Contracts.Models.IHardware?> CreateAsync(HardwareEntity entity, bool withPassword, CancellationToken cancellationToken = default)
  {
    HardwareBase? hardwareBase;
    MqttEntity? mqttEntity;
    switch ((Business.Contracts.Models.HardwareType?)entity.Type)
    {
      case Business.Contracts.Models.HardwareType.MQTT:
        mqttEntity = await mqttRepository.GetAsync(entity.Id, cancellationToken);
        if (mqttEntity is null)
          throw new ArgumentException(HardwareConfigurationNotFound, nameof(entity));

        var mqttConfiguration = new MqttConfiguration()
        {
          Address = mqttEntity.Address,
          Password = GetMqttPassword(mqttEntity.Password, configuration),
          Port = mqttEntity.Port,
          SubscriptionsIn = [],
          SubscriptionsOut = [],
          UserName = mqttEntity.Username,
          UseTLS = mqttEntity.UseTLS == 1
        };

        hardwareBase = new Mqtt(mqttConfiguration, configuration.SslCertificate)
        {
          Id = entity.Id,
          Name = entity.Name,
          Enabled = entity.Enabled != 0,
          Order = entity.Order,
          Configuration = entity.Configuration ?? throw new ArgumentException(ConfigurationIsNull, nameof(entity))
        };
        hardwareBase.Configuration = entity.Configuration;
        break;
      case Business.Contracts.Models.HardwareType.MQTTTasmota:
        mqttEntity = await mqttRepository.GetAsync(entity.Id, cancellationToken);
        if (mqttEntity is null)
          throw new ArgumentException(HardwareConfigurationNotFound, nameof(entity));

        var tasmotaConfiguration = new MqttConfiguration()
        {
          Address = mqttEntity.Address,
          Password = GetMqttPassword(mqttEntity.Password, configuration),
          Port = mqttEntity.Port,
          SubscriptionsIn = [entity.Configuration!],
          SubscriptionsOut = [],
          UserName = mqttEntity.Username,
          UseTLS = mqttEntity.UseTLS == 1
        };

        hardwareBase = new MqttTasmota(tasmotaConfiguration, configuration.SslCertificate)
        {
          Id = entity.Id,
          Name = entity.Name,
          Enabled = entity.Enabled != 0,
          Order = entity.Order,
          Configuration = JsonSerializer.Serialize(tasmotaConfiguration.Clone(withPassword))
        };
        break;
      case Business.Contracts.Models.HardwareType.Dummy:
        hardwareBase = new Dummy
        {
          Id = entity.Id,
          Name = entity.Name,
          Enabled = entity.Enabled != 0,
          Order = entity.Order,
          Configuration = entity.Configuration
        };
        break;
      default:
        hardwareBase = null;
        break;
    }

    if (hardwareBase is null)
      return null;

    hardwareBase.LogLevel = (LogLevel)entity.LogLevel;
    return hardwareBase;
  }

  public async Task<Business.Contracts.Models.IHardware?> CreateAsync(CreateHardwareParams request, CancellationToken cancellationToken = default)
  {
    HardwareBase? hardwareBase;
    MqttConfiguration? mqttConf;

    switch (request.Type)
    {
      case Business.Contracts.Models.HardwareType.MQTT:
        mqttConf = GetMqttConfiguration(request.Configuration);

        hardwareBase = new Mqtt(mqttConf, configuration.SslCertificate)
        {
          Id = request.Id,
          Name = request.Name,
          Enabled = request.Enabled,
          Order = request.Order
        };

        await mqttRepository.CreateAsync(hardwareBase, cancellationToken);
        break;
      case Business.Contracts.Models.HardwareType.MQTTTasmota:
        mqttConf = GetMqttConfiguration(request.Configuration);

        hardwareBase = new MqttTasmota(mqttConf, configuration.SslCertificate)
        {
          Id = request.Id,
          Name = request.Name,
          Enabled = request.Enabled,
          Order = request.Order,
          Configuration = mqttConf.SubscriptionsIn[0]
        };
        await mqttRepository.CreateAsync(hardwareBase, cancellationToken);
        break;
      case Business.Contracts.Models.HardwareType.Dummy:
        hardwareBase = new Dummy
        {
          Id = request.Id,
          Name = request.Name,
          Enabled = request.Enabled,
          Order = request.Order,
          Configuration = request.Configuration
        };
        break;
      default:
        hardwareBase = null;
        break;
    }
    if (hardwareBase is null)
      return null;

    hardwareBase.LogLevel = request.LogLevel;
    return hardwareBase;
  }
}
