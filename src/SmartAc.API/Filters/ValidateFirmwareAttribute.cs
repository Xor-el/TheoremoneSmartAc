using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace SmartAc.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateFirmwareAttribute : ActionFilterAttribute
{
    private const string SmVerRegexString = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

    private readonly Regex _regex =
        new(SmVerRegexString, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string firmwareVersion = context.HttpContext.Request.Query["firmwareVersion"];

        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            return;
        }

        if (!_regex.IsMatch(firmwareVersion))
        {
            context.ModelState.AddModelError(
                "firmwareVersion",
                "The firmware value does not match semantic versioning format.");

            var vpd = new ValidationProblemDetails(context.ModelState);

            context.Result = new BadRequestObjectResult(vpd);
        }
    }
}
