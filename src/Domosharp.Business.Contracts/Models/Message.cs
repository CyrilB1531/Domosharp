namespace Domosharp.Business.Contracts.Models;

public class Message(MessageType type, Device device, string command, int? value) : IMessage
{
  public MessageType Type { get; private set; } = type;
  public string Command { get; private set; } = command;
  public int? Value { get; private set; } = value;
  public Device Device { get; private set; } = device;
}
