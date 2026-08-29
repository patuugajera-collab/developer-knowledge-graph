using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class HealthServiceTests
{
    [Fact]
    public async Task GetHealthAsync_ReturnsHealthy_WhenConnected()
    {
        var connection = Substitute.For<IDatabaseConnection>();
        connection.IsHealthyAsync(Arg.Any<CancellationToken>()).Returns(true);

        var service = new HealthService(connection);
        var result = await service.GetHealthAsync(CancellationToken.None);

        result.Status.Should().Be("healthy");
        result.Database.Should().Be("SQL Server");
        result.Message.Should().BeNull();
    }

    [Fact]
    public async Task GetHealthAsync_ReportsUnreachable_WhenDatabaseDown()
    {
        var connection = Substitute.For<IDatabaseConnection>();
        connection.IsHealthyAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = new HealthService(connection);
        var result = await service.GetHealthAsync(CancellationToken.None);

        result.Status.Should().Be("unreachable");
        result.Message.Should().Be("Unable to connect to the database. Please try again.");
    }
}