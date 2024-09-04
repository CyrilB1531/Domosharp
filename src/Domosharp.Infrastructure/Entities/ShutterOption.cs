namespace Domosharp.Infrastructure.Entities
{
  internal class ShutterOption
  {
    
    public ShutterOption(byte value) {
      Invert = (value & 1) != 0;
      Lock = (value & 2) != 0;
      ExtraEndStop = (value & 4) != 0;
      InvertWebButtons = (value & 8) != 0;
      ExtraStopRelay = (value & 16) != 0;
    }

    public bool Invert {  get; init; }

    public bool Lock { get; init; }

    public bool ExtraEndStop { get; init; }
    public bool InvertWebButtons { get; init; }
    public bool ExtraStopRelay { get; init; }
    
    public byte GetValue()
    {
      byte value = 0;
      if (Invert)
        value |= 1;
      if (Lock)
        value |= 2;
      if (ExtraEndStop)
        value |= 4;
      if (InvertWebButtons)
        value |= 8;
      if (ExtraStopRelay)
        value |= 16;

      return value;
    }
  }
}
