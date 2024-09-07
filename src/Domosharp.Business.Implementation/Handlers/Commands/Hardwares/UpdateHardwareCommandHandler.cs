using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

using System.Transactions;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class UpdateHardwareCommandHandler(
    IHardwareRepository hardwareRepository,
    IMqttRepository mqttRepository,
    IMainWorker mainWorker
  ) : IRequestHandler<UpdateHardwareCommand, bool>
{
  public async Task<bool> Handle(UpdateHardwareCommand request, CancellationToken cancellationToken)
  {
    var hardware = await hardwareRepository.GetAsync(request.Id, true, cancellationToken);
    if (hardware is null)
      return false;

    var hasChanges = false;
    if (hardware.Name != request.Name)
    {
      hardware.Name = request.Name;
      hasChanges = true;
    }
    if (hardware.Enabled != request.Enabled)
    {
      hardware.Enabled = request.Enabled;
      hasChanges = true;
    }
    if (hardware.LogLevel != request.LogLevel)
    {
      hardware.LogLevel = request.LogLevel;
      hasChanges = true;
    }
    if (hardware.Order != request.Order)
    {
      hardware.Order = request.Order;
      hasChanges = true;
    }
    if (hardware.Configuration != request.Configuration)
    {
      hardware.Configuration = request.Configuration;
      hasChanges = true;
    }

    if (!hasChanges)
      return false;

    using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    if (hardware.Type is Contracts.Models.HardwareType.MQTT or Contracts.Models.HardwareType.MQTTTasmota &&
      !await mqttRepository.UpdateAsync(hardware, cancellationToken))
      return false;
    if (!await hardwareRepository.UpdateAsync(hardware, cancellationToken))
      return false;
    transactionScope.Complete();
    mainWorker.UpdateHardware(hardware);

    return true;
  }
}
