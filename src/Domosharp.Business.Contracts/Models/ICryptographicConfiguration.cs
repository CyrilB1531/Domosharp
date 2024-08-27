namespace Domosharp.Business.Contracts.Models
{
  public interface ICryptographicConfiguration
  {
    string Key { get; }

    string IV { get; }

    byte[] KeyBytes();

    byte[] IVBytes();
  }
}
