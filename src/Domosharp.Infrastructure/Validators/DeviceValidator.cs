using Domosharp.Business.Contracts.Models;

using FluentValidation;

namespace Domosharp.Infrastructure.Validators;

public class DeviceValidator : AbstractValidator<Device>
{
  public DeviceValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithErrorCode("ArgumentException").WithMessage("Name cannot be null or empty");
    RuleFor(x => x.DeviceId).NotEmpty().WithErrorCode("ArgumentException").WithMessage("DeviceId cannot be null or empty");
    RuleFor(x => x.BatteryLevel).InclusiveBetween(0,100).WithErrorCode("ArgumentOutOfRangeException").WithMessage("BatteryLevel must be between 0 and 100");
    RuleFor(x => x.SignalLevel).LessThan(0).WithErrorCode("ArgumentOutOfRangeException").WithMessage("SignalLevel must be less than 0");
    RuleFor(x => x.Order).GreaterThanOrEqualTo(0).WithErrorCode("ArgumentOutOfRangeException").WithMessage("Order must be greater or equal to 0");
  }
}
