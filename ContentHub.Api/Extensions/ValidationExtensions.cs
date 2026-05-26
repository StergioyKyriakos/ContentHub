using System.Reflection;
using FluentValidation;

namespace ContentHub.Api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddContentHubValidation(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}