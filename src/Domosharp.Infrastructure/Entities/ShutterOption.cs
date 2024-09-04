namespace Domosharp.Infrastructure.Entities
{
  internal class ShutterOption(byte value)
  {
    public bool Invert { get; init; } = (value & 1) != 0;

    public bool Lock { get; init; } = (value & 2) != 0;

    public bool ExtraEndStop { get; init; } = (value & 4) != 0;
    public bool InvertWebButtons { get; init; } = (value & 8) != 0;
    public bool ExtraStopRelay { get; init; } = (value & 16) != 0;

    public byte GetValue()
    {
      byte newValue = 0;
      if (Invert)
        newValue |= 1;
      if (Lock)
        newValue |= 2;
      if (ExtraEndStop)
        newValue |= 4;
      if (InvertWebButtons)
        newValue |= 8;
      if (ExtraStopRelay)
        newValue |= 16;

      return newValue;
    }
  }
}
