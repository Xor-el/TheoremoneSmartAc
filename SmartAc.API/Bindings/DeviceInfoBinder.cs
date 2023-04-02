using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SmartAc.API.Bindings;

public class DeviceInfoBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.FieldName != "serialNumber")
        {
            return Task.CompletedTask;
        }

        var serialNumber = bindingContext.ActionContext.HttpContext.User.Identity?.Name ?? string.Empty;

        bindingContext.Result = ModelBindingResult.Success(serialNumber);

        return Task.CompletedTask;
    }
}