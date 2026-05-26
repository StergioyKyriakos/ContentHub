using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;

namespace ContentHub.ArchitectureTests;

public sealed class NamingConventionTests
{
    [Fact]
    public void Endpoint_Classes_Should_End_With_Endpoint()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Api)
            .That()
            .ResideInNamespace("ContentHub.Api.Features")
            .And()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .HaveNameEndingWith("Endpoint")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Classes_Implementing_IEndpointDefinition_Should_End_With_Endpoint()
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
            type => type.Name.EndsWith("Endpoint"),
            "all endpoint definition classes should end with Endpoint");
    }

    [Fact]
    public void Command_Classes_Should_End_With_Command()
    {
        var commandTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Namespace?.Contains(".Features.") == true &&
                type.Name.Contains("Command", StringComparison.Ordinal) &&
                !type.Name.EndsWith("Validator"))
            .ToList();

        commandTypes.Should().OnlyContain(
            type => type.Name.EndsWith("Command"),
            "commands should use the Command suffix");
    }

    [Fact]
    public void Query_Classes_Should_End_With_Query()
    {
        var queryTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Namespace?.Contains(".Features.") == true &&
                type.Name.Contains("Query", StringComparison.Ordinal) &&
                !type.Name.EndsWith("Validator"))
            .ToList();

        queryTypes.Should().OnlyContain(
            type => type.Name.EndsWith("Query"),
            "queries should use the Query suffix");
    }

    [Fact]
    public void Validator_Classes_Should_End_With_Validator()
    {
        var validatorTypes = ArchitectureTestAssemblies.Api
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        validatorTypes.Should().OnlyContain(
            type => type.Name.EndsWith("Validator"),
            "validators should use the Validator suffix");
    }
}