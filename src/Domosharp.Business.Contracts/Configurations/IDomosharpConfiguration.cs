using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Contracts.Configurations;

public interface IDomosharpConfiguration
{
  IWebBindings? Web { get; }
  IWebBindings? Ssl { get; }

  string? SslCertificate { get; }

  ICryptographicConfiguration Aes { get; }
}
