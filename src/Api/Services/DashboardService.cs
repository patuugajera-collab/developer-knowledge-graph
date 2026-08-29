using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct);
}

public sealed class DashboardService : IDashboardService
{
    private readonly IGraphRepository _repository;

    public DashboardService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct)
    {
        var counts = await _repository.GetDashboardCountsAsync(ct);
        var statuses = await _repository.GetProjectStatusCountsAsync(ct);
        var relationshipTypes = await _repository.GetRelationshipTypeCountsAsync(ct);
        var topTechnologies = await _repository.GetCentralTechnologiesAsync(8, ct);

        return new DashboardStatsDto(
            counts.Developers,
            counts.Projects,
            counts.Technologies,
            counts.Repositories,
            counts.Tasks,
            counts.Organizations,
            counts.Relationships,
            counts.AverageConnectionsPerDeveloper,
            statuses,
            relationshipTypes,
            topTechnologies);
    }
}