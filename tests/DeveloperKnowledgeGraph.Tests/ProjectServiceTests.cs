using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class ProjectServiceTests
{
    private readonly IGraphRepository _repository = Substitute.For<IGraphRepository>();
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _service = new ProjectService(_repository);
    }

    [Fact]
    public async Task GetRecommendedDevelopersAsync_ReturnsMultiHopMatches()
    {
        var matches = new[]
        {
            new RecommendedDeveloperDto("d1", "Alice Chen", "Backend Engineer", 3, 3, 1.0, "Acme Corp"),
            new RecommendedDeveloperDto("d8", "Liam O'Brien", "Backend Engineer", 2, 3, 0.67, null),
        };

        _repository.NodeExistsAsync("Project", "p2", Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetRecommendedDevelopersAsync("p2", Arg.Any<CancellationToken>()).Returns(matches);

        var result = await _service.GetRecommendedDevelopersAsync("p2", CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Coverage.Should().Be(1.0);
        result[1].Coverage.Should().Be(0.67);
    }

    [Fact]
    public async Task GetRecommendedDevelopersAsync_Throws404_WhenProjectMissing()
    {
        _repository.NodeExistsAsync("Project", "missing", Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _service.GetRecommendedDevelopersAsync("missing", CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetProjectDependenciesAsync_PassesNormalizedDepth()
    {
        _repository.NodeExistsAsync("Project", "p1", Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetProjectDependenciesAsync("p1", 5, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<ProjectDependencyDto>)Array.Empty<ProjectDependencyDto>());

        await _service.GetProjectDependenciesAsync("p1", null, CancellationToken.None);

        await _repository.Received(1).GetProjectDependenciesAsync("p1", 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectDependenciesAsync_RejectsInvalidDepth()
    {
        _repository.NodeExistsAsync("Project", "p1", Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _service.GetProjectDependenciesAsync("p1", 99, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetProjectDependenciesAsync("p1", Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectContributorsAsync_ReturnsContributors()
    {
        var contributors = new[]
        {
            new ProjectContributorDto("d1", "Alice Chen", "Backend Engineer", "atlas-api", 120, "2021"),
        };

        _repository.NodeExistsAsync("Project", "p1", Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetProjectContributorsAsync("p1", Arg.Any<CancellationToken>()).Returns(contributors);

        var result = await _service.GetProjectContributorsAsync("p1", CancellationToken.None);

        result.Should().ContainSingle().Which.RepositoryName.Should().Be("atlas-api");
    }
}