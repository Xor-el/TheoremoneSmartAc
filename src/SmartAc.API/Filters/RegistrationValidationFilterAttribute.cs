using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace SmartAc.API.Filters;

public sealed class RegistrationValidationFilterAttribute : IActionFilter
{
    private const string SmVerRegexString = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

    private readonly Regex _regex =
        new(SmVerRegexString, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    public void OnActionExecuting(ActionExecutingContext context)
    {
        string firmwareVersion = context.HttpContext.Request.Query["firmwareVersion"];

        if (context.ModelState.IsValid && _regex.IsMatch(firmwareVersion)) return;

        context.ModelState.AddModelError(
            "firmwareVersion", 
            "The firmware value does not match semantic versioning format.");

        var vp = new ValidationProblemDetails(context.ModelState);

        context.Result = new BadRequestObjectResult(vp);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
