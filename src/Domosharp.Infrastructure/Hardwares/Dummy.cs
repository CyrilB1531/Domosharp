namespace Domosharp.Infrastructure.Hardwares
{
  internal record Dummy : HardwareBase
  {
    public Dummy()
    {
      Type = Business.Contracts.Models.HardwareType.Dummy;
    }
  }
}
