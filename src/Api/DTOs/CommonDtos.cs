namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ErrorResponse(string Message, string? Detail, string TraceId);

public sealed record HealthResponse(string Status, string Database, string? Message);

public sealed record DashboardCounts(
    int Developers,
    int Projects,
    int Technologies,
    int Repositories,
    int Tasks,
    int Organizations,
    int Relationships,
    double AverageConnectionsPerDeveloper);