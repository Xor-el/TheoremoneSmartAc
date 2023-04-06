using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartAc.API.Data;
using SmartAc.API.Identity;
using SmartAc.API.Middlewares;
using SmartAc.Application.Options;
using SmartAc.Infrastructure.Repositories;
using FluentValidation;
using SmartAc.Application.Abstractions.Services;

namespace SmartAc.API;

internal static class ConfigurationExtensions
{
    public static void AddSmartAcServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<IAuthorizationHandler, ValidTokenAuthorizationHandler>();

        services.TryAddTransient<GlobalExceptionHandlingMiddleware>();

        services
            .AddOptions<SensorParams>()
            .BindConfiguration("SensorParams");

        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
    }

    public static void AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartAC API",
                Description = "SmartAC Device Reporting API",
                Version = "v1"
            });

            c.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "BearerAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = configuration["Jwt:Key"];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        var jwtService =
            services
                .BuildServiceProvider()
                .GetRequiredService<ISmartAcJwtService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("DeviceAdmin", policy =>
                policy.RequireRole(jwtService.JwtScopeDeviceAdminService)
            );

            options.AddPolicy("DeviceIngestion", policy =>
            {
                policy.RequireRole(jwtService.JwtScopeDeviceIngestionService);
                policy.AddRequirements(new ValidTokenRequirement());
            });
        });

        services.AddHttpContextAccessor();
    }

    public static void UseOpenApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartAC API V1"); });
    }

    public static void MapSmartAcControllers(this WebApplication app)
    {
        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    }

    public static void EnsureDatabaseSetup(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<SmartAcContext>();
        db.Database.EnsureCreated();
        SmartAcDataSeeder.Seed(db);
    }
}