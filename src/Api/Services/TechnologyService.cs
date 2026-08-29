using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface ITechnologyService
{
    Task<PaginatedResponse<TechnologySummaryDto>> GetTechnologiesAsync(string? search, string? category, int? page, int? pageSize, CancellationToken ct);

    Task<TechnologyDetailDto> GetTechnologyAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<TechnologyDeveloperDto>> GetTechnologyDevelopersAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<TechnologyProjectDto>> GetTechnologyProjectsAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<string>> GetTechnologyCategoriesAsync(CancellationToken ct);
}

public sealed class TechnologyService : ITechnologyService
{
    private readonly IGraphRepository _repository;

    public TechnologyService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<TechnologySummaryDto>> GetTechnologiesAsync(
        string? search, string? category, int? page, int? pageSize, CancellationToken ct)
    {
        var (safePage, safeSize, skip) = PaginationHelper.Normalize(page, pageSize);
        var (total, items) = await _repository.SearchTechnologiesAsync(search?.Trim() ?? string.Empty, category, skip, safeSize, ct);
        return PaginationHelper.Build(items, safePage, safeSize, total);
    }

    public async Task<TechnologyDetailDto> GetTechnologyAsync(string id, CancellationToken ct)
    {
        await EnsureTechnologyExistsAsync(id, ct);
        return await _repository.GetTechnologyByIdAsync(id, ct)
               ?? throw new EntityNotFoundException("Technology", id);
    }

    public async Task<IReadOnlyList<TechnologyDeveloperDto>> GetTechnologyDevelopersAsync(string id, CancellationToken ct)
    {
        await EnsureTechnologyExistsAsync(id, ct);
        return await _repository.GetTechnologyDevelopersAsync(id, ct);
    }

    public async Task<IReadOnlyList<TechnologyProjectDto>> GetTechnologyProjectsAsync(string id, CancellationToken ct)
    {
        await EnsureTechnologyExistsAsync(id, ct);
        return await _repository.GetTechnologyProjectsAsync(id, ct);
    }

    public async Task<IReadOnlyList<string>> GetTechnologyCategoriesAsync(CancellationToken ct)
    {
        return await _repository.GetTechnologyCategoriesAsync(ct);
    }

    private async Task EnsureTechnologyExistsAsync(string id, CancellationToken ct)
    {
        if (!await _repository.NodeExistsAsync("Technology", id, ct))
        {
            throw new EntityNotFoundException("Technology", id);
        }
    }
}