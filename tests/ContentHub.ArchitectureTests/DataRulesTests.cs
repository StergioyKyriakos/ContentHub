using ContentHub.Data.Entities.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.ArchitectureTests;

public sealed class DataRulesTests
{
    [Fact]
    public void Entities_Should_Reside_In_Entities_Namespace()
    {
        var entityTypes = ArchitectureTestAssemblies.Data
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                (
                    typeof(Entity).IsAssignableFrom(type) ||
                    type.Namespace?.Contains(".Entities.") == true
                ))
            .ToList();

        entityTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Data.Entities.") == true,
            "entities should live under ContentHub.Data.Entities");
    }

    [Fact]
    public void Dtos_Should_Reside_In_Dtos_Namespace()
    {
        var dtoTypes = ArchitectureTestAssemblies.Data
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                (
                    type.Name.EndsWith("Dto") ||
                    type.Name.EndsWith("Request") ||
                    type.Name.EndsWith("Response")
                ))
            .ToList();

        dtoTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Data.Dtos.") == true,
            "shared DTOs should live under ContentHub.Data.Dtos");
    }

    [Fact]
    public void Ef_Configurations_Should_Reside_In_Data_Project()
    {
        var configurationTypes = ArchitectureTestAssemblies.Data
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(IsEntityTypeConfiguration))
            .ToList();

        configurationTypes.Should().OnlyContain(
            type => type.Namespace != null && type.Namespace.StartsWith("ContentHub.Data.") == true,
            "EF configurations should stay inside the Data project");
    }

    [Fact]
    public void DbContext_Should_Reside_In_Data_Persistence_Namespace()
    {
        var dbContextTypes = ArchitectureTestAssemblies.Data
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(DbContext).IsAssignableFrom(type))
            .ToList();

        dbContextTypes.Should().OnlyContain(
            type => type.Namespace == "ContentHub.Data.Persistence",
            "DbContext classes should live under ContentHub.Data.Persistence");
    }

    private static bool IsEntityTypeConfiguration(Type interfaceType)
    {
        return interfaceType.IsGenericType &&
               interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>);
    }
}