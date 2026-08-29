namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record DeveloperSummaryDto(
    string Id,
    string Name,
    string Email,
    string Role,
    string? OrganizationName);

public sealed record DeveloperDetailDto(
    string Id,
    string Name,
    string Email,
    string Role,
    string? OrganizationName,
    int ProjectCount,
    int SkillCount,
    int RepositoryCount);

public sealed record DeveloperProjectDto(
    string ProjectId,
    string ProjectName,
    string ProjectStatus,
    string Role,
    string Since);

public sealed record DeveloperSkillDto(
    string TechnologyId,
    string TechnologyName,
    string Category,
    string Proficiency,
    string Since);

public sealed record DeveloperRepositoryDto(
    string RepositoryId,
    string RepositoryName,
    string Url,
    int ContributionCount,
    string Since);