using ContentHub.Api.Features.Categories.CreateCategory;
using FluentAssertions;

namespace ContentHub.UnitTests.Application.Validation;

public sealed class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Technology",
            Slug = "technology",
            Description = "Tech posts",
            DisplayOrder = 1,
            IsVisible = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateCategoryCommand
        {
            Name = "",
            Slug = "technology",
            DisplayOrder = 1,
            IsVisible = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_Should_Fail_When_DisplayOrder_Is_Negative()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Technology",
            Slug = "technology",
            DisplayOrder = -1,
            IsVisible = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateCategoryCommand.DisplayOrder));
    }
}