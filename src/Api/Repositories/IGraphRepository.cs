using DeveloperKnowledgeGraph.Api.DTOs;

namespace DeveloperKnowledgeGraph.Api.Repositories;

public interface IGraphRepository
{
    Task<DashboardCounts> GetDashboardCountsAsync(CancellationToken ct);

    Task<IReadOnlyList<ProjectStatusCountDto>> GetProjectStatusCountsAsync(CancellationToken ct);

    Task<IReadOnlyList<RelationshipTypeCountDto>> GetRelationshipTypeCountsAsync(CancellationToken ct);

    Task<IReadOnlyList<CentralTechnologyDto>> GetCentralTechnologiesAsync(int limit, CancellationToken ct);

    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit, CancellationToken ct);

    Task<(int Total, IReadOnlyList<DeveloperSummaryDto> Items)> SearchDevelopersAsync(string search, int skip, int limit, CancellationToken ct);

    Task<(int Total, IReadOnlyList<ProjectSummaryDto> Items)> SearchProjectsAsync(string search, string? status, int skip, int limit, CancellationToken ct);

    Task<(int Total, IReadOnlyList<TechnologySummaryDto> Items)> SearchTechnologiesAsync(string search, string? category, int skip, int limit, CancellationToken ct);

    Task<DeveloperDetailDto?> GetDeveloperByIdAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperProjectDto>> GetDeveloperProjectsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperSkillDto>> GetDeveloperSkillsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperRepositoryDto>> GetDeveloperRepositoriesAsync(string id, CancellationToken ct);

    Task<ProjectDetailDto?> GetProjectByIdAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectDependencyDto>> GetProjectDependenciesAsync(string id, int maxDepth, CancellationToken ct);

    Task<IReadOnlyList<ProjectTechnologyDto>> GetProjectTechnologiesAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectDeveloperDto>> GetProjectDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectRepositoryDto>> GetProjectRepositoriesAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<IndirectTechnologyDto>> GetProjectIndirectTechnologiesAsync(string id, int maxDepth, CancellationToken ct);

    Task<IReadOnlyList<RecommendedDeveloperDto>> GetRecommendedDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectContributorDto>> GetProjectContributorsAsync(string id, CancellationToken ct);

    Task<TechnologyDetailDto?> GetTechnologyByIdAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<TechnologyDeveloperDto>> GetTechnologyDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<TechnologyProjectDto>> GetTechnologyProjectsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<string>> GetTechnologyCategoriesAsync(CancellationToken ct);

    Task<IReadOnlyList<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct);

    Task<IReadOnlyList<DeveloperSummaryDto>> GetOrganizationDevelopersAsync(string id, CancellationToken ct);

    Task<GraphResponseDto?> GetGraphAsync(string id, int maxDepth, CancellationToken ct);

    Task<ShortestPathDto?> GetShortestPathAsync(string developerId, string projectId, CancellationToken ct);

    Task<bool> NodeExistsAsync(string label, string id, CancellationToken ct);
}