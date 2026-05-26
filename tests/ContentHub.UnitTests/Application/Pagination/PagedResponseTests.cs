using ContentHub.Data.Dtos.Common;
using FluentAssertions;

namespace ContentHub.UnitTests.Application.Pagination;

public sealed class PagedResponseTests
{
    [Fact]
    public void Create_Should_Calculate_TotalPages()
    {
        var items = new[] { 1, 2, 3 };

        var response = PagedResponse<int>.Create(
            items,
            page: 1,
            pageSize: 10,
            totalItems: 25);

        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public void Create_Should_Have_NextPage_When_More_Items_Exist()
    {
        var response = PagedResponse<int>.Create(
            [1, 2, 3],
            page: 1,
            pageSize: 10,
            totalItems: 25);

        response.HasNextPage.Should().BeTrue();
        response.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Have_PreviousPage_When_Page_Is_Greater_Than_One()
    {
        var response = PagedResponse<int>.Create(
            [1, 2, 3],
            page: 2,
            pageSize: 10,
            totalItems: 25);

        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_Not_Have_NextPage_On_Last_Page()
    {
        var response = PagedResponse<int>.Create(
            [1, 2, 3],
            page: 3,
            pageSize: 10,
            totalItems: 25);

        response.HasNextPage.Should().BeFalse();
        response.HasPreviousPage.Should().BeTrue();
    }
}