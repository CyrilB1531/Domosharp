using Domosharp.Business.Contracts.Configurations;

namespace Domosharp.Business.Implementation.Configurations
{
  public record WebBindings : IWebBindings
  {
    public string Address { get; set; } = string.Empty;

    public int Port { get; set; }
  }
}
