using Domosharp.Business.Contracts.Models;

using NSubstitute;

using System.Collections;

namespace Domosharp.Infrastructure.Tests.HostedServices.Data;

public class ProcessLoopData : IEnumerable<object[]>
{
  public IEnumerator<object[]> GetEnumerator()
  {
    var hardaware = Substitute.For<IHardware>();
    var device = new Device
    {
      Hardware = hardaware,
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

  IEnumerator IEnumerable.GetEnumerator()
  {
    throw new NotImplementedException();
  }
}
