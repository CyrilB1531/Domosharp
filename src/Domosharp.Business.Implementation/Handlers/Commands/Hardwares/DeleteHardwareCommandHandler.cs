using Domosharp.Business.Contracts.Commands.Hardwares;
using Domosharp.Business.Contracts.HostedServices;
using Domosharp.Business.Contracts.Repositories;

using MediatR;

using System.Transactions;

namespace Domosharp.Business.Implementation.Handlers.Commands.Hardwares;

public class DeleteHardwareCommandHandler(
    IHardwareRepository hardwareRepository,
    IMqttRepository mqttRepository,
    IMainWorker mainWorker
  ) : IRequestHandler<DeleteHardwareCommand, bool>
{
  public async Task<bool> Handle(DeleteHardwareCommand request, CancellationToken cancellationToken)
  {
    using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    var hardware = await hardwareRepository.GetAsync(request.Id, false, cancellationToken);
    if (hardware is null)
      return false;

    if (hardware.Type is Contracts.Models.HardwareType.MQTTTasmota or Contracts.Models.HardwareType.MQTT
      && !await mqttRepository.DeleteAsync(request.Id, cancellationToken))
      return false;

    if (!await hardwareRepository.DeleteAsync(request.Id, cancellationToken))
      return false;
    mainWorker.DeleteHardware(request.Id);
    transactionScope.Complete();
    return true;
  }
}
