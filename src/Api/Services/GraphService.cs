using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IGraphService
{
    Task<GraphResponseDto> GetGraphAsync(string entityType, string id, int? maxDepth, CancellationToken ct);

    Task<ShortestPathDto> GetShortestPathAsync(string developerId, string projectId, CancellationToken ct);

    Task<IReadOnlyList<CentralTechnologyDto>> GetCentralTechnologiesAsync(int limit, CancellationToken ct);
}

public sealed class GraphService : IGraphService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Developer", "Project", "Technology",
    };

    private readonly IGraphRepository _repository;

    public GraphService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<GraphResponseDto> GetGraphAsync(string entityType, string id, int? maxDepth, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entityType) || !AllowedTypes.Contains(entityType))
        {
            throw new ValidationException("entityType must be one of: Developer, Project, Technology.");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("id is required.");
        }

        var canonicalType = AllowedTypes.First(t => string.Equals(t, entityType, StringComparison.OrdinalIgnoreCase));
        if (!await _repository.NodeExistsAsync(canonicalType, id, ct))
        {
            throw new EntityNotFoundException(canonicalType, id);
        }

        var depth = DepthGuard.Normalize(maxDepth);
        var result = await _repository.GetGraphAsync(id, depth, ct);
        return result ?? throw new EntityNotFoundException(canonicalType, id);
    }

    public async Task<ShortestPathDto> GetShortestPathAsync(string developerId, string projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(developerId) || string.IsNullOrWhiteSpace(projectId))
        {
            throw new ValidationException("developerId and projectId are required.");
        }

        if (!await _repository.NodeExistsAsync("Developer", developerId, ct))
        {
            throw new EntityNotFoundException("Developer", developerId);
        }

        if (!await _repository.NodeExistsAsync("Project", projectId, ct))
        {
            throw new EntityNotFoundException("Project", projectId);
        }

        var path = await _repository.GetShortestPathAsync(developerId, projectId, ct);
        if (path is null)
        {
            throw new EntityNotFoundException("path", $"{developerId}-{projectId}");
        }

        return path;
    }

    public async Task<IReadOnlyList<CentralTechnologyDto>> GetCentralTechnologiesAsync(int limit, CancellationToken ct)
    {
        return await _repository.GetCentralTechnologiesAsync(limit, ct);
    }
}