using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Contracts.Models;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;

namespace Domosharp.Business.Implementation.Configurations;

public class DomosharpConfiguration : IDomosharpConfiguration
{
  public static void CheckCryptographicConfiguration(IConfigurationRoot configuration)
  {
    if (configuration["Aes:Key"] is null)
    {
      Aes aes = System.Security.Cryptography.Aes.Create();
      aes.GenerateKey();
      aes.GenerateIV();

      var fileName = Path.Combine(Thread.GetDomain().BaseDirectory, "appsettings.json");
      var file = File.ReadAllText(fileName);
      var content = JObject.Parse(file);
      var itemToAdd = new JObject
      {
        ["Key"] = Convert.ToBase64String(aes.Key),
        ["IV"] = Convert.ToBase64String(aes.IV)
      };
      content.Add(nameof(Aes), itemToAdd);
      var c = File.CreateText(fileName);
      c.Write(JsonConvert.SerializeObject(content, Formatting.Indented));
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
