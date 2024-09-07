using Domosharp.Business.Contracts.Models;

namespace Domosharp.Api.Models
{
  public record DeviceResponse
  {
    public DeviceResponse(Device device)
    {
      Id = device.Id;
      HardwareId = device.HardwareId;
      DeviceId = device.DeviceId;
      Name = device.Name;
      Active = device.Active;
      Type = device.Type;
      Favorite = device.Favorite;
      SignalLevel = device.SignalLevel;
      BatteryLevel = device.BatteryLevel;
      Order = device.Order;
      Protected = device.Protected;
      SpecificParameters = device.SpecificParameters;
      Value = device.Value;
      Index = device.Index;
    }

    public int Id { get; init; }

    public int HardwareId { get; init; }

    public string? DeviceId { get; init; }

    public string? Name { get; init; }

    public bool Active { get; init; }

    public DeviceType Type { get; init; }

    public bool Favorite { get; init; }

    public int SignalLevel { get; init; }

    public int BatteryLevel { get; init; }

    public int Order { get; init; }

    public bool Protected { get; init; }

    public string? SpecificParameters { get; init; }

    public decimal? Value { get; init; }

    public int? Index { get; init; }
  }
}
