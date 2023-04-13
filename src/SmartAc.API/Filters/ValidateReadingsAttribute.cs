using System.Collections.Immutable;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.API.Contracts;

namespace SmartAc.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateReadingsAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var readings = context.ActionArguments
            .FirstOrDefault(a => a.Value is IEnumerable<SensorReading>)
            .Value as IEnumerable<SensorReading>;

        var validator =
            context.HttpContext.RequestServices.GetRequiredService<IValidator<SensorReading>>();

        var tasks = readings!.Select(x => validator.ValidateAsync(x));

        var results = await Task.WhenAll(tasks);

        var failures =
            results
                .SelectMany(x => x.Errors)
                .AsParallel()
                .GroupBy(x => x.PropertyName)
                .ToImmutableDictionary(g =>
                    g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        if (!failures.Any())
        {
            await next();
            return;
        }

        var vpd = new ValidationProblemDetails(failures);
        context.Result = new BadRequestObjectResult(vpd);
    }
}