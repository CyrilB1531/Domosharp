using Domosharp.Business.Contracts.Models;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Domosharp.Infrastructure.Tests.HostedServices.Data;

public class ProcessLoopData : IEnumerable<object[]>
{
  public IEnumerator<object[]> GetEnumerator()
  {
    var device = new Device
    {
      HardwareId = 1,
      Active = true
    };
    yield return new object[]
    {
          new Message(MessageType.SendValue, device, "Send", 10)
    };
    yield return new object[]
    {
          new Message(MessageType.UpdateValue, device, string.Empty, 10)
    };
  }

  [ExcludeFromCodeCoverage]
  IEnumerator IEnumerable.GetEnumerator()
  {
    throw new NotImplementedException();
  }
}
