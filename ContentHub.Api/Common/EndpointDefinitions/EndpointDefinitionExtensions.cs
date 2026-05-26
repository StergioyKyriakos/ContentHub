using System.Reflection;

namespace ContentHub.Api.Common.EndpointDefinitions;

public static class EndpointDefinitionExtensions
{
    public static IServiceCollection AddEndpointDefinitions(
        this IServiceCollection services,
        Assembly assembly)
    {
        var endpointDefinitions = assembly
            .DefinedTypes
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                typeof(IEndpointDefinition).IsAssignableFrom(type))
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>()
            .ToArray();

        services.AddSingleton(endpointDefinitions);

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointDefinitions = app.Services.GetRequiredService<IEndpointDefinition[]>();

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.MapEndpoints(app);
        }

        return app;
    }
}