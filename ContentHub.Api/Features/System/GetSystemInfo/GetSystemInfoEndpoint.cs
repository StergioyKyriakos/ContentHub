using ContentHub.Api.Common.ApiResults;
using ContentHub.Api.Common.EndpointDefinitions;

namespace ContentHub.Api.Features.System.GetSystemInfo;

public sealed class GetSystemInfoEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/system/info", Handle)
            .WithTags("System")
            .WithName("GetSystemInfo");
    }

    private static IResult Handle(IWebHostEnvironment environment)
    {
        var response = new GetSystemInfoResponse
        {
            ApplicationName = "ContentHub",
            Version = "1.0.0",
            Environment = environment.EnvironmentName,
            MachineName = Environment.MachineName,
            ServerTimeUtc = DateTime.UtcNow
        };

        return ResultsFactory.Ok(response);
    }
}