using ContentHub.Application.Common.Slugs;
using FluentAssertions;

namespace ContentHub.UnitTests.Application.Slugs;

public sealed class SlugGeneratorTests
{
    private readonly SlugGenerator _slugGenerator = new();

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Hello   World  ", "hello-world")]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("ASP.NET Core API", "asp-net-core-api")]
    [InlineData("Café au lait", "cafe-au-lait")]
    [InlineData("Multiple---Dashes", "multiple-dashes")]
    public void Generate_Should_Return_Expected_Slug(
        string value,
        string expected)
    {
        var result = _slugGenerator.Generate(value);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Generate_Should_Return_Empty_When_Input_Is_Empty(
        string? value)
    {
        var result = _slugGenerator.Generate(value!);

        result.Should().BeEmpty();
    }
}