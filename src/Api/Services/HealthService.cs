using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IHealthService
{
    Task<HealthResponse> GetHealthAsync(CancellationToken ct);
}

public sealed class HealthService : IHealthService
{
    private const string DatabaseName = "SQL Server";

    private readonly IDatabaseConnection _connection;

    public HealthService(IDatabaseConnection connection)
    {
        _connection = connection;
    }

    public async Task<HealthResponse> GetHealthAsync(CancellationToken ct)
    {
        var healthy = await _connection.IsHealthyAsync(ct);

        return healthy
            ? new HealthResponse("healthy", DatabaseName, null)
            : new HealthResponse("unreachable", DatabaseName, "Unable to connect to the database. Please try again.");
    }
}