using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IProjectService
{
    Task<PaginatedResponse<ProjectSummaryDto>> GetProjectsAsync(string? search, string? status, int? page, int? pageSize, CancellationToken ct);

    Task<ProjectDetailDto> GetProjectAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectDependencyDto>> GetProjectDependenciesAsync(string id, int? maxDepth, CancellationToken ct);

    Task<IReadOnlyList<ProjectTechnologyDto>> GetProjectTechnologiesAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectDeveloperDto>> GetProjectDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectRepositoryDto>> GetProjectRepositoriesAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<IndirectTechnologyDto>> GetProjectIndirectTechnologiesAsync(string id, int? maxDepth, CancellationToken ct);

    Task<IReadOnlyList<RecommendedDeveloperDto>> GetRecommendedDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<ProjectContributorDto>> GetProjectContributorsAsync(string id, CancellationToken ct);
}

public sealed class ProjectService : IProjectService
{
    private readonly IGraphRepository _repository;

    public ProjectService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<ProjectSummaryDto>> GetProjectsAsync(
        string? search, string? status, int? page, int? pageSize, CancellationToken ct)
    {
        var (safePage, safeSize, skip) = PaginationHelper.Normalize(page, pageSize);
        var (total, items) = await _repository.SearchProjectsAsync(search?.Trim() ?? string.Empty, status, skip, safeSize, ct);
        return PaginationHelper.Build(items, safePage, safeSize, total);
    }

    public async Task<ProjectDetailDto> GetProjectAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectByIdAsync(id, ct)
               ?? throw new EntityNotFoundException("Project", id);
    }

    public async Task<IReadOnlyList<ProjectDependencyDto>> GetProjectDependenciesAsync(string id, int? maxDepth, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        var depth = DepthGuard.NormalizeDependencyDepth(maxDepth);
        return await _repository.GetProjectDependenciesAsync(id, depth, ct);
    }

    public async Task<IReadOnlyList<ProjectTechnologyDto>> GetProjectTechnologiesAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectTechnologiesAsync(id, ct);
    }

    public async Task<IReadOnlyList<ProjectDeveloperDto>> GetProjectDevelopersAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectDevelopersAsync(id, ct);
    }

    public async Task<IReadOnlyList<ProjectRepositoryDto>> GetProjectRepositoriesAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectRepositoriesAsync(id, ct);
    }

    public async Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectTasksAsync(id, ct);
    }

    public async Task<IReadOnlyList<IndirectTechnologyDto>> GetProjectIndirectTechnologiesAsync(string id, int? maxDepth, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        var depth = DepthGuard.NormalizeDependencyDepth(maxDepth);
        return await _repository.GetProjectIndirectTechnologiesAsync(id, depth, ct);
    }

    public async Task<IReadOnlyList<RecommendedDeveloperDto>> GetRecommendedDevelopersAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetRecommendedDevelopersAsync(id, ct);
    }

    public async Task<IReadOnlyList<ProjectContributorDto>> GetProjectContributorsAsync(string id, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(id, ct);
        return await _repository.GetProjectContributorsAsync(id, ct);
    }

    private async Task EnsureProjectExistsAsync(string id, CancellationToken ct)
    {
        if (!await _repository.NodeExistsAsync("Project", id, ct))
        {
            throw new EntityNotFoundException("Project", id);
        }
    }
}