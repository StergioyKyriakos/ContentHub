namespace ContentHub.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddContentHubHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }

    public static WebApplication MapContentHubHealthChecks(
        this WebApplication app)
    {
        app.MapHealthChecks("/health")
            .WithTags("Health")
            .WithName("HealthCheck");

        app.MapGet("/health/live", () =>
                Results.Ok(new
                {
                    status = "Healthy",
                    check = "live"
                }))
            .WithTags("Health")
            .WithName("LiveHealthCheck");

        app.MapGet("/health/ready", () =>
                Results.Ok(new
                {
                    status = "Healthy",
                    check = "ready"
                }))
            .WithTags("Health")
            .WithName("ReadyHealthCheck");

        return app;
    }
}