namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record TechnologySummaryDto(string Id, string Name, string Category);

public sealed record TechnologyDetailDto(
    string Id,
    string Name,
    string Category,
    int ProjectCount,
    int DeveloperCount);

public sealed record TechnologyDeveloperDto(
    string DeveloperId,
    string Name,
    string Role,
    string Proficiency,
    string Since);

public sealed record TechnologyProjectDto(
    string ProjectId,
    string ProjectName,
    string ProjectStatus,
    string? Purpose);

public sealed record CentralTechnologyDto(
    string Id,
    string Name,
    string Category,
    int ProjectUsage,
    int SkillCount,
    int Centrality);

public sealed record OrganizationSummaryDto(string Id, string Name, int DeveloperCount, int ProjectCount);