using System.Reflection;
using FluentAssertions;

namespace ContentHub.ArchitectureTests;

public sealed class EndpointRulesTests
{
    [Fact]
    public void Endpoint_Classes_Should_Not_Have_Too_Many_Methods()
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

        var violatingTypes = endpointTypes
            .Where(type =>
                type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Count(method => !method.IsSpecialName) > 8)
            .Select(type => type.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            "endpoint classes should stay small and focused");
    }

    [Fact]
    public void Endpoint_Source_Files_Should_Not_Exceed_Reasonable_Line_Count()
    {
        var solutionRoot = FindSolutionRoot();

        var endpointFiles = Directory
            .GetFiles(
                Path.Combine(solutionRoot, "ContentHub.Api", "Features"),
                "*Endpoint.cs",
                SearchOption.AllDirectories)
            .ToList();

        var violatingFiles = endpointFiles
            .Select(file => new
            {
                File = file,
                LineCount = File.ReadAllLines(file).Length
            })
            .Where(file => file.LineCount > 250)
            .ToList();

        violatingFiles.Should().BeEmpty(
            "endpoint files should not become giant god classes. Violations: {0}",
            string.Join(Environment.NewLine, violatingFiles.Select(x => $"{x.File} has {x.LineCount} lines")));
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (directory.GetFiles("ContentHub.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find solution root.");
    }
}