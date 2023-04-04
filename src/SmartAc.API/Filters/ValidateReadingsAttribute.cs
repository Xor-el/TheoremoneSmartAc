using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.Application.Contracts;

namespace SmartAc.API.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateReadingsAttribute : Attribute, IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments["sensorReadings"] is not List<SensorReading> readings)
            {
                context.Result = new BadRequestResult();
                return Task.CompletedTask;
            }

            var validator = context
                .HttpContext
                .RequestServices
                .GetRequiredService<IValidator<SensorReading>>();

            var hasValidationErrors =
                readings
                    .Select(x => validator.Validate(x))
                    .SelectMany(x => x.Errors)
                    .Any();

            if (hasValidationErrors)
            {
                context.Result = new UnprocessableEntityResult();
                return Task.CompletedTask;
            }

            next();
            return Task.CompletedTask;
        }
    }
}
