using System.Net;
using System.Net.Http.Json;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Health_ReportsHealthy_WhenConnected()
    {
        var factory = new WebTestFactory(services =>
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true)));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Status.Should().Be("healthy");
        body.Database.Should().Be("SQL Server");
    }

    [Fact]
    public async Task Health_Returns503_WhenDatabaseUnavailable()
    {
        var factory = new WebTestFactory(services =>
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(false)));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        body!.Status.Should().Be("unreachable");
        body.Message.Should().Be("Unable to connect to the database. Please try again.");
    }

    [Fact]
    public async Task Developers_Endpoint_ReturnsPaginatedList()
    {
        var repository = Substitute.For<IGraphRepository>();
        repository.SearchDevelopersAsync(Arg.Any<string>(), 0, 20, Arg.Any<CancellationToken>())
            .Returns((
                1,
                (IReadOnlyList<DeveloperSummaryDto>)new[]
                {
                    new DeveloperSummaryDto("d1", "Alice Chen", "alice.example@test.dev", "Backend Engineer", "Acme Corp"),
                }));

        var factory = new WebTestFactory(services =>
        {
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true));
            services.ReplaceWith(repository);
        });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/developers?search=alice");
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<DeveloperSummaryDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Items.Should().ContainSingle().Which.Name.Should().Be("Alice Chen");
        body.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ProjectLookup_Returns404_WhenMissing()
    {
        var repository = Substitute.For<IGraphRepository>();
        repository.NodeExistsAsync("Project", "missing", Arg.Any<CancellationToken>()).Returns(false);

        var factory = new WebTestFactory(services =>
        {
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true));
            services.ReplaceWith(repository);
        });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/projects/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Message.Should().Contain("not found");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task MultiHopRecommendedDevelopers_ReturnsMatches()
    {
        var repository = Substitute.For<IGraphRepository>();
        repository.NodeExistsAsync("Project", "p2", Arg.Any<CancellationToken>()).Returns(true);
        repository.GetRecommendedDevelopersAsync("p2", Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<RecommendedDeveloperDto>)new[]
            {
                new RecommendedDeveloperDto("d1", "Alice Chen", "Backend Engineer", 3, 3, 1.0, "Acme Corp"),
            });

        var factory = new WebTestFactory(services =>
        {
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true));
            services.ReplaceWith(repository);
        });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/projects/p2/recommended-developers");
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<RecommendedDeveloperDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().ContainSingle().Which.Coverage.Should().Be(1.0);
    }

    [Fact]
    public async Task Developers_Endpoint_Returns503WithSafeMessage_WhenDatabaseUnavailable()
    {
        var repository = Substitute.For<IGraphRepository>();
        repository.SearchDevelopersAsync(Arg.Any<string>(), 0, 20, Arg.Any<CancellationToken>())
            .Returns<(int Total, IReadOnlyList<DeveloperSummaryDto> Items)>(_ =>
                throw new DeveloperKnowledgeGraph.Api.Exceptions.DatabaseUnavailableException("boom"));

        var factory = new WebTestFactory(services =>
        {
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true));
            services.ReplaceWith(repository);
        });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/developers");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Message.Should().Be("Unable to connect to the database. Please try again.");
        error.Message.Should().NotContain("connection string");
    }

    [Fact]
    public async Task UnknownEndpoint_Returns404()
    {
        var factory = new WebTestFactory(services =>
            services.ReplaceWith<IDatabaseConnection>(new StubConnection(true)));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/definitely-not-a-route");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}