namespace Domosharp.Infrastructure.Entities
{
  internal record TasmotaShutterPayload
  {
    public int Position { get; set; }
    public int Direction { get; set; }
    public int Target { get; set; }
    public int Tilt { get; set; }
  }
}
