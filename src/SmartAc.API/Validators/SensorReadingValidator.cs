using FluentValidation;
using SmartAc.Application.Contracts;

namespace SmartAc.API.Validators;

internal sealed class SensorReadingValidator : AbstractValidator<SensorReading>
{
    public SensorReadingValidator()
    {
        RuleFor(x => x.RecordedDateTime)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(x => x.Temperature).PrecisionScale(5, 2, true);
        RuleFor(x => x.CarbonMonoxide).PrecisionScale(5, 2, true);
        RuleFor(x => x.Humidity).PrecisionScale(5, 2, true);
        RuleFor(x => x.Health).IsInEnum();
    }
}