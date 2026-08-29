namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record SearchItemDto(string Id, string Name, string Subtitle, string Type);

public sealed record SearchGroupDto(string Category, IReadOnlyList<SearchItemDto> Results);

public sealed record SearchResponseDto(IReadOnlyList<SearchGroupDto> Groups, int Total);

public sealed record SearchResultDto(string Id, string Name, string Type, string Subtitle);

public sealed record DashboardStatsDto(
    int Developers,
    int Projects,
    int Technologies,
    int Repositories,
    int Tasks,
    int Organizations,
    int Relationships,
    double AverageConnectionsPerDeveloper,
    IReadOnlyList<ProjectStatusCountDto> ProjectStatus,
    IReadOnlyList<RelationshipTypeCountDto> RelationshipTypes,
    IReadOnlyList<CentralTechnologyDto> TopTechnologies);

public sealed record ProjectStatusCountDto(string Status, int Count);

public sealed record RelationshipTypeCountDto(string Type, int Count);