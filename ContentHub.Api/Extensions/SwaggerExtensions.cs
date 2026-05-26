using Microsoft.OpenApi;

namespace ContentHub.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddContentHubSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ContentHub API",
                Version = "v1",
                Description = "Production-ready CMS API."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token only. Example: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            
            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    new List<string>(Array.Empty<string>())
                }
            });
        });

        return services;
    }

    public static WebApplication UseContentHubSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ContentHub API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}