namespace ContentHub.Api.Common.EndpointDefinitions;

public interface IEndpointDefinition
{
    void MapEndpoints(IEndpointRouteBuilder app);
}