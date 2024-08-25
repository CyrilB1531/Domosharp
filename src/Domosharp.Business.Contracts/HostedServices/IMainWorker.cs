using Domosharp.Business.Contracts.Models;

using Microsoft.Extensions.Hosting;

namespace Domosharp.Business.Contracts.HostedServices
{
  public interface IMainWorker : IHostedService
  {
    void AddHardware(IHardware hardware);
    void UpdateHardware(IHardware hardware);
    void DeleteHardware(int hardwareId);
  }
}
