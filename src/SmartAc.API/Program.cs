using SmartAc.API;
using System.Text.Json.Serialization;
using SmartAc.API.Middlewares;
using SmartAc.Application.Extensions;
using SmartAc.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSmartAcServices(builder.Configuration);

builder.Services.AddOpenApiDocumentation();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.EnsureDatabaseSetup();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseOpenApiDocumentation();

app.UseAuthentication();

app.UseAuthorization();

app.MapSmartAcControllers();

app.Run();
