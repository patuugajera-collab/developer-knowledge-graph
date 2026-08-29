using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class PaginationHelperTests
{
    [Fact]
    public void Normalize_Defaults_WhenNull()
    {
        var (page, pageSize, skip) = PaginationHelper.Normalize(null, null);

        page.Should().Be(1);
        pageSize.Should().Be(PaginationHelper.DefaultPageSize);
        skip.Should().Be(0);
    }

    [Fact]
    public void Normalize_ClampsOutOfRange()
    {
        var (page, pageSize, _) = PaginationHelper.Normalize(0, 10_000);
        page.Should().Be(1);
        pageSize.Should().Be(PaginationHelper.MaxPageSize);
    }

    [Fact]
    public void Normalize_ComputesSkip()
    {
        var (page, pageSize, skip) = PaginationHelper.Normalize(3, 10);
        (page, pageSize, skip).Should().Be((3, 10, 20));
    }

    [Fact]
    public void Build_ComputesTotalPages()
    {
        var result = PaginationHelper.Build(new[] { "a", "b", "c" }, 1, 10, 27);
        result.TotalCount.Should().Be(27);
        result.TotalPages.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void Build_EmptyData_ReturnsZeroPages()
    {
        var result = PaginationHelper.Build<DeveloperSummaryDto>(Array.Empty<DeveloperSummaryDto>(), 1, 10, 0);
        result.TotalPages.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}