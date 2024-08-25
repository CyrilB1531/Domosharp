namespace Domosharp.Business.Contracts.Models;

public interface IMessage
{
  MessageType Type { get; }
  string Command { get; }
  int? Value { get; }
  Device Device { get; }
}
