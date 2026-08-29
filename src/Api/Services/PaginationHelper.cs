using DeveloperKnowledgeGraph.Api.DTOs;

namespace DeveloperKnowledgeGraph.Api.Services;

public static class PaginationHelper
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    private const int MaxPage = 100000;

    public static (int Page, int PageSize, int Skip) Normalize(int? page, int? pageSize)
    {
        var safePage = Math.Clamp(page ?? 1, 1, MaxPage);
        var safeSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        return (safePage, safeSize, (safePage - 1) * safeSize);
    }

    public static PaginatedResponse<T> Build<T>(IReadOnlyList<T> items, int page, int pageSize, int total)
    {
        var totalPages = (int)Math.Ceiling(total / (double)Math.Max(1, pageSize));
        return new PaginatedResponse<T>(items, page, pageSize, total, totalPages);
    }
}