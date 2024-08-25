namespace Domosharp.Business.Contracts.Models;

public class DeviceEventArgs(Device device) : EventArgs
{
  public Device Device { get; set; } = device;
}
