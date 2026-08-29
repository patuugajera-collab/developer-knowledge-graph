using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class DeveloperServiceTests
{
    private readonly IGraphRepository _repository = Substitute.For<IGraphRepository>();
    private readonly DeveloperService _service;

    public DeveloperServiceTests()
    {
        _service = new DeveloperService(_repository);
    }

    [Fact]
    public async Task GetDevelopersAsync_ReturnsPaginatedResult()
    {
        var developers = new[]
        {
            new DeveloperSummaryDto("d1", "Alice Chen", "alice@example.com", "Backend Engineer", "Acme Corp"),
            new DeveloperSummaryDto("d2", "Marcus Johnson", "marcus@example.com", "Frontend Engineer", null),
        };

        _repository.SearchDevelopersAsync(Arg.Any<string>(), 0, 20, Arg.Any<CancellationToken>())
            .Returns((550, (IReadOnlyList<DeveloperSummaryDto>)developers));

        var result = await _service.GetDevelopersAsync(null, 1, 20, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(550);
        result.TotalPages.Should().Be(28);
        await _repository.Received(1).SearchDevelopersAsync(string.Empty, 0, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDevelopersAsync_ForwardsSearchTerm()
    {
        _repository.SearchDevelopersAsync(Arg.Any<string>(), 0, 20, Arg.Any<CancellationToken>())
            .Returns((0, (IReadOnlyList<DeveloperSummaryDto>)Array.Empty<DeveloperSummaryDto>()));

        await _service.GetDevelopersAsync("   alice   ", 1, 20, CancellationToken.None);

        await _repository.Received(1)
            .SearchDevelopersAsync("alice", 0, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDeveloperAsync_Throws404_WhenMissing()
    {
        _repository.NodeExistsAsync("Developer", "nope", Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _service.GetDeveloperAsync("nope", CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetDeveloperAsync_ReturnsDetail()
    {
        var detail = new DeveloperDetailDto("d1", "Alice Chen", "alice@example.com", "Backend Engineer", "Acme Corp", 3, 4, 2);
        _repository.NodeExistsAsync("Developer", "d1", Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetDeveloperByIdAsync("d1", Arg.Any<CancellationToken>()).Returns(detail);

        var result = await _service.GetDeveloperAsync("d1", CancellationToken.None);

        result.Should().Be(detail);
    }

    [Fact]
    public async Task GetDeveloperProjectsAsync_ReturnsEmpty_WhenNoProjects()
    {
        _repository.NodeExistsAsync("Developer", "d9", Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetDeveloperProjectsAsync("d9", Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<DeveloperProjectDto>)Array.Empty<DeveloperProjectDto>());

        var result = await _service.GetDeveloperProjectsAsync("d9", CancellationToken.None);

        result.Should().BeEmpty();
    }
}