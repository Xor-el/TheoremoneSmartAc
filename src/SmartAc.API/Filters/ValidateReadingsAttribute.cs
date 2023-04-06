using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.Application.Contracts;
using System.Collections.Immutable;

namespace SmartAc.API.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateReadingsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var readings = 
                context.ActionArguments["sensorReadings"] as IEnumerable<SensorReading>;

            var validator =
                context.HttpContext.RequestServices.GetRequiredService<IValidator<SensorReading>>();

            var errors =
                readings?
                    .Select(x => validator.Validate(x))
                    .SelectMany(x => x.Errors)
                    .GroupBy(x => x.PropertyName)
                    .ToImmutableDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());

            if (errors?.Any() == true)
            {
                var vpd = new ValidationProblemDetails(errors);
                context.Result = new BadRequestObjectResult(vpd);
            }
        }
    }
}
