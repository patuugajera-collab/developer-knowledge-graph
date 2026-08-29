using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface ISearchService
{
    Task<SearchResponseDto> SearchAsync(string query, int? limit, CancellationToken ct);
}

public sealed class SearchService : ISearchService
{
    private const int DefaultLimit = 25;
    private const int MaxLimit = 50;

    public static readonly IReadOnlyDictionary<string, string> TypeOrder = new Dictionary<string, string>
    {
        ["Developer"] = "Developers",
        ["Project"] = "Projects",
        ["Technology"] = "Technologies",
        ["Repository"] = "Repositories",
    };

    private readonly IGraphRepository _repository;

    public SearchService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<SearchResponseDto> SearchAsync(string query, int? limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchResponseDto(Array.Empty<SearchGroupDto>(), 0);
        }

        var safeLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);

        var items = await _repository.SearchAsync(query.Trim(), safeLimit, ct);

        var grouping = new Dictionary<string, int> { ["Developers"] = 0, ["Projects"] = 1, ["Technologies"] = 2, ["Repositories"] = 3 };

        var grouped = items
            .GroupBy(n => TypeOrder.TryGetValue(n.Type, out var order) ? order : n.Type)
            .Select(g => new SearchGroupDto(
                g.Key,
                g.Select(n => new SearchItemDto(n.Id, n.Name, n.Subtitle, n.Type)).ToList()))
            .OrderBy(g => grouping.TryGetValue(g.Category, out var rank) ? rank : int.MaxValue)
            .ToList();

        var total = grouped.Sum(g => g.Results.Count);
        return new SearchResponseDto(grouped, total);
    }
}