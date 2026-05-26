using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Enums;
using FluentAssertions;

namespace ContentHub.UnitTests.Data.Entities.Posts;

public sealed class PostTests
{
    [Fact]
    public void New_Post_Should_Start_As_Draft()
    {
        var post = CreatePost();

        post.Status.Should().Be(PostStatus.Draft);
        post.IsFeatured.Should().BeFalse();
        post.PublishedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Publish_Should_Set_Status_To_Published_And_Set_PublishedAt()
    {
        var post = CreatePost();

        post.Publish();

        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAtUtc.Should().NotBeNull();
        post.ScheduledForUtc.Should().BeNull();
    }

    [Fact]
    public void Publish_Should_Throw_When_Post_Is_Archived()
    {
        var post = CreatePost();

        post.Archive();

        var action = () => post.Publish();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Archived posts cannot be published directly.");
    }

    [Fact]
    public void Schedule_Should_Set_Status_To_Scheduled()
    {
        var post = CreatePost();

        var scheduledFor = DateTime.UtcNow.AddDays(1);

        post.Schedule(scheduledFor);

        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledForUtc.Should().Be(scheduledFor);
        post.IsFeatured.Should().BeFalse();
    }

    [Fact]
    public void Schedule_Should_Throw_When_Date_Is_In_The_Past()
    {
        var post = CreatePost();

        var scheduledFor = DateTime.UtcNow.AddDays(-1);

        var action = () => post.Schedule(scheduledFor);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Scheduled date must be in the future.");
    }

    [Fact]
    public void SetFeatured_Should_Throw_When_Post_Is_Not_Published()
    {
        var post = CreatePost();

        var action = () => post.SetFeatured();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only published posts can be featured.");
    }

    [Fact]
    public void SetFeatured_Should_Work_When_Post_Is_Published()
    {
        var post = CreatePost();

        post.Publish();
        post.SetFeatured();

        post.IsFeatured.Should().BeTrue();
        post.FeaturedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Unpublish_Should_Remove_Featured_State()
    {
        var post = CreatePost();

        post.Publish();
        post.SetFeatured();

        post.Unpublish();

        post.Status.Should().Be(PostStatus.Draft);
        post.IsFeatured.Should().BeFalse();
        post.FeaturedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Archive_Should_Remove_Featured_State()
    {
        var post = CreatePost();

        post.Publish();
        post.SetFeatured();

        post.Archive();

        post.Status.Should().Be(PostStatus.Archived);
        post.IsFeatured.Should().BeFalse();
        post.FeaturedAtUtc.Should().BeNull();
    }

    private static Post CreatePost()
    {
        return new Post(
            title: "Test Post",
            slug: "test-post",
            summary: "Test summary",
            content: "Test content",
            createdById: Guid.NewGuid());
    }
}