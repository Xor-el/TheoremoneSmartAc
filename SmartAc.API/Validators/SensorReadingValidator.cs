using FluentValidation;
using SmartAc.Application.Contracts;

namespace SmartAc.API.Validators;

internal sealed class SensorReadingValidator : AbstractValidator<SensorReading>
{
    public SensorReadingValidator()
    {
        RuleFor(x => x.RecordedDateTime)
            .NotEmpty();

        RuleFor(x => x.Temperature)
            .NotEmpty()
            .PrecisionScale(5, 2, true);

        RuleFor(x => x.CarbonMonoxide)
            .NotEmpty()
            .PrecisionScale(5, 2, true);

        RuleFor(x => x.Humidity)
            .NotEmpty()
            .PrecisionScale(5, 2, true);

        RuleFor(x => x.Health).IsInEnum();
    }
}