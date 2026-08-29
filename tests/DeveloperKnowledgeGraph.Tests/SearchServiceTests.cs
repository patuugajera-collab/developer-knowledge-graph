using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class SearchServiceTests
{
    private readonly IGraphRepository _repository = Substitute.For<IGraphRepository>();
    private readonly SearchService _service;

    public SearchServiceTests()
    {
        _service = new SearchService(_repository);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var result = await _service.SearchAsync("   ", null, CancellationToken.None);

        result.Total.Should().Be(0);
        result.Groups.Should().BeEmpty();
        await _repository.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_GroupsResultsByType()
    {
        var developer = new SearchResultDto("d1", "Alice Chen", "Developer", "Backend Engineer");
        var project = new SearchResultDto("p2", "Nova Analytics Platform", "Project", "active");

        _repository.SearchAsync("nova", 25, Arg.Any<CancellationToken>()).Returns(new[] { developer, project });

        var result = await _service.SearchAsync("nova", null, CancellationToken.None);

        result.Total.Should().Be(2);
        result.Groups.Should().HaveCount(2);
        result.Groups[0].Category.Should().Be("Developers");
        result.Groups[0].Results.Should().ContainSingle().Which.Name.Should().Be("Alice Chen");
        result.Groups[1].Category.Should().Be("Projects");
        result.Groups[1].Results[0].Subtitle.Should().Be("active");
    }

    [Fact]
    public async Task SearchAsync_ClampsLimit()
    {
        _repository.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<SearchResultDto>());

        await _service.SearchAsync("x", 1000, CancellationToken.None);

        await _repository.Received(1).SearchAsync("x", 50, Arg.Any<CancellationToken>());
    }
}