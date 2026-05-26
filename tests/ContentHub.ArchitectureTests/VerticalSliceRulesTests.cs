using FluentAssertions;

namespace ContentHub.ArchitectureTests;

public sealed class VerticalSliceRulesTests
{
    [Fact]
    public void Feature_Endpoint_Classes_Should_Reside_Under_Features_Namespace()
    {
        var endpointDefinitionType = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Single(type => type.Name == "IEndpointDefinition");

        var endpointTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                endpointDefinitionType.IsAssignableFrom(type))
            .ToList();

        endpointTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Api.Features.") == true,
            "endpoint classes should be organized in vertical slices under Features");
    }

    [Fact]
    public void Feature_Command_Query_And_Validator_Classes_Should_Reside_Under_Features_Namespace()
    {
        var relevantTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                (
                    type.Name.EndsWith("Command") ||
                    type.Name.EndsWith("Query") ||
                    type.Name.EndsWith("Validator")
                ))
            .ToList();

        relevantTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Api.Features.") == true,
            "commands, queries and validators should stay inside feature slices");
    }

    [Fact]
    public void Feature_Response_Classes_Should_Reside_Under_Features_Namespace()
    {
        var responseTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Name.EndsWith("Response"))
            .ToList();

        responseTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Api.Features.") == true,
            "feature-specific responses should stay inside feature slices");
    }
}