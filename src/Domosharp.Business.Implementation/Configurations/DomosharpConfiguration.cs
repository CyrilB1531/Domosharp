using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Models;

using Microsoft.Extensions.Configuration;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Domosharp.Business.Implementation.Configurations;

public class DomosharpConfiguration : IDomosharpConfiguration
{
  private readonly static JsonSerializerOptions _jsonFileSerializerOptions = new () {  WriteIndented = true };
  public static void CheckCryptographicConfiguration(IConfigurationRoot configuration)
  {
    if (configuration["Aes:Key"] is null)
    {
      Aes aes = System.Security.Cryptography.Aes.Create();
      aes.GenerateKey();
      aes.GenerateIV();

      var fileName = Path.Combine(Thread.GetDomain().BaseDirectory, "appsettings.json");
      var file = File.ReadAllText(fileName);
      var content = JsonNode.Parse(file)!.AsObject()!;

      content.Add(nameof(Aes), new JsonObject([
          new KeyValuePair<string, JsonNode?>("Key", JsonValue.Create(Convert.ToBase64String(aes.Key))),
          new KeyValuePair<string, JsonNode?>("IV", JsonValue.Create(Convert.ToBase64String(aes.IV)))
        ]));
      var c = File.CreateText(fileName);
      c.Write(JsonSerializer.Serialize(content, _jsonFileSerializerOptions));
      c.Close();
      c.Dispose();
      configuration.Reload();
    }
  }

  public IWebBindings? Web { get; set; }

  public IWebBindings? Ssl { get; set; }

  public string? SslCertificate { get; set; }

  public ICryptographicConfiguration Aes { get; set; } = new CryptographicConfiguration();
}
