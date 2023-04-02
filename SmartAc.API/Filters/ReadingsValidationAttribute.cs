using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.Application.Contracts;

namespace SmartAc.API.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReadingsValidationAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments["sensorReadings"] is not List<SensorReading> readings)
            {
                context.Result = new BadRequestResult();
                return;
            }

            var validator = context
                .HttpContext
                .RequestServices
                .GetRequiredService<IValidator<SensorReading>>();

            var validationResults =
                readings
                    .Select(x => validator.Validate(x))
                    .SelectMany(x => x.Errors);

            if (validationResults.Any())
            {
                context.Result = new UnprocessableEntityResult();
                return;
            }

            await next();
        }
    }
}
