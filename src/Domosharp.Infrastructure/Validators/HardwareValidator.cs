using Domosharp.Business.Contracts.Models;
using FluentValidation;

namespace Domosharp.Infrastructure.Validators;

public class HardwareValidator : AbstractValidator<IHardware>
{
  public HardwareValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithErrorCode("ArgumentException").WithMessage("Name cannot be null or empty");
    RuleFor(x => x.Order).GreaterThanOrEqualTo(0).WithErrorCode("ArgumentOutOfRangeException").WithMessage("Order must be greater or equal to 0");
  }
}
