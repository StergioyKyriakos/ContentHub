using ContentHub.Api.Common.Middleware;

namespace ContentHub.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseContentHubMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}