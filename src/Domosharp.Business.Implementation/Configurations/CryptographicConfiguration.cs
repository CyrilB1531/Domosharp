using Domosharp.Business.Contracts.Models;

namespace Domosharp.Business.Implementation.Configurations
{
  public record CryptographicConfiguration : ICryptographicConfiguration
  {
    public string Key { get; set; } = string.Empty;

    public string IV { get; set; } = string.Empty;

    public byte[] KeyBytes() => Convert.FromBase64String(Key);

    public byte[] IVBytes() => Convert.FromBase64String(IV);
  }
}
