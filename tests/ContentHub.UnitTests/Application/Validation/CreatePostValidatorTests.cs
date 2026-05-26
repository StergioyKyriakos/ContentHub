using ContentHub.Api.Features.Posts.CreatePost;
using FluentAssertions;

namespace ContentHub.UnitTests.Application.Validation;

public sealed class CreatePostValidatorTests
{
    private readonly CreatePostValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Slug = "test-post",
            Summary = "Test summary",
            Content = "Test content",
            CategoryIds = [Guid.NewGuid()],
            AuthorIds = [Guid.NewGuid()],
            Tags = ["test", "cms"]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Title_Is_Empty()
    {
        var command = new CreatePostCommand
        {
            Title = "",
            Content = "Test content"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePostCommand.Title));
    }

    [Fact]
    public void Validate_Should_Fail_When_Content_Is_Empty()
    {
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Content = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePostCommand.Content));
    }

    [Fact]
    public void Validate_Should_Fail_When_Tag_Is_Too_Long()
    {
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Content = "Test content",
            Tags = [new string('a', 101)]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.Contains(nameof(CreatePostCommand.Tags)));
    }
}