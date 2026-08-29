using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IDeveloperService
{
    Task<PaginatedResponse<DeveloperSummaryDto>> GetDevelopersAsync(string? search, int? page, int? pageSize, CancellationToken ct);

    Task<DeveloperDetailDto> GetDeveloperAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperProjectDto>> GetDeveloperProjectsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperSkillDto>> GetDeveloperSkillsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<DeveloperRepositoryDto>> GetDeveloperRepositoriesAsync(string id, CancellationToken ct);
}

public sealed class DeveloperService : IDeveloperService
{
    private readonly IGraphRepository _repository;

    public DeveloperService(IGraphRepository repository)
    {
        _repository = repository;
    }

    private static string NormalizeSearch(string? search) => search?.Trim() ?? string.Empty;

    public async Task<PaginatedResponse<DeveloperSummaryDto>> GetDevelopersAsync(
        string? search, int? page, int? pageSize, CancellationToken ct)
    {
        var (safePage, safeSize, skip) = PaginationHelper.Normalize(page, pageSize);
        var (total, items) = await _repository.SearchDevelopersAsync(NormalizeSearch(search), skip, safeSize, ct);
        return PaginationHelper.Build(items, safePage, safeSize, total);
    }

    public async Task<DeveloperDetailDto> GetDeveloperAsync(string id, CancellationToken ct)
    {
        await EnsureDeveloperExistsAsync(id, ct);
        return await _repository.GetDeveloperByIdAsync(id, ct)
               ?? throw new EntityNotFoundException("Developer", id);
    }

    public async Task<IReadOnlyList<DeveloperProjectDto>> GetDeveloperProjectsAsync(string id, CancellationToken ct)
    {
        await EnsureDeveloperExistsAsync(id, ct);
        return await _repository.GetDeveloperProjectsAsync(id, ct);
    }

    public async Task<IReadOnlyList<DeveloperSkillDto>> GetDeveloperSkillsAsync(string id, CancellationToken ct)
    {
        await EnsureDeveloperExistsAsync(id, ct);
        return await _repository.GetDeveloperSkillsAsync(id, ct);
    }

    public async Task<IReadOnlyList<DeveloperRepositoryDto>> GetDeveloperRepositoriesAsync(string id, CancellationToken ct)
    {
        await EnsureDeveloperExistsAsync(id, ct);
        return await _repository.GetDeveloperRepositoriesAsync(id, ct);
    }

    private async Task EnsureDeveloperExistsAsync(string id, CancellationToken ct)
    {
        if (!await _repository.NodeExistsAsync("Developer", id, ct))
        {
            throw new EntityNotFoundException("Developer", id);
        }
    }
}