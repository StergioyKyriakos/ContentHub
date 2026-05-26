using FluentAssertions;
using NetArchTest.Rules;

namespace ContentHub.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private const string ApiNamespace = "ContentHub.Api";
    private const string ApplicationNamespace = "ContentHub.Application";
    private const string DataNamespace = "ContentHub.Data";
    private const string InfrastructureNamespace = "ContentHub.Infrastructure";

    [Fact]
    public void Data_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Data)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ContentHub.Data must not reference ContentHub.Api.");
    }

    [Fact]
    public void Data_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Data)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ContentHub.Data must not reference ContentHub.Infrastructure.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ContentHub.Application must not reference ContentHub.Api.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ContentHub.Application must not reference ContentHub.Infrastructure.");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(ArchitectureTestAssemblies.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ContentHub.Infrastructure must not reference ContentHub.Api.");
    }

    [Fact]
    public void Api_Can_Depend_On_Application_Data_And_Infrastructure()
    {
        var referencedAssemblyNames = ArchitectureTestAssemblies.Api
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        referencedAssemblyNames.Should().Contain("ContentHub.Application");
        referencedAssemblyNames.Should().Contain("ContentHub.Data");
        referencedAssemblyNames.Should().Contain("ContentHub.Infrastructure");
    }

    [Fact]
    public void Infrastructure_Can_Depend_On_Application_And_Data()
    {
        var referencedAssemblyNames = ArchitectureTestAssemblies.Infrastructure
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        referencedAssemblyNames.Should().Contain("ContentHub.Application");
        referencedAssemblyNames.Should().Contain("ContentHub.Data");
    }

    [Fact]
    public void Application_Can_Depend_On_Data()
    {
        var referencedAssemblyNames = ArchitectureTestAssemblies.Application
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        referencedAssemblyNames.Should().Contain("ContentHub.Data");
    }
}