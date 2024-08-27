using Domosharp.Business.Contracts.Configurations;
using Domosharp.Business.Implementation.Configurations;

using NSubstitute;

using System.Security.Cryptography;

namespace Domosharp.Common.Tests;

public static class DomosharpConfigurationBuilder
{
  public static IDomosharpConfiguration Build()
  {
    var aes = Aes.Create();
    aes.GenerateKey();
    aes.GenerateIV();

    var key = Convert.ToBase64String(aes.Key);
    var iv = Convert.ToBase64String(aes.IV);

    var domosharpConfiguration = Substitute.For<IDomosharpConfiguration>();
    domosharpConfiguration.Aes.Returns(new CryptographicConfiguration()
    {
      Key = key,
      IV = iv,
    });

    return domosharpConfiguration;
  }
}

