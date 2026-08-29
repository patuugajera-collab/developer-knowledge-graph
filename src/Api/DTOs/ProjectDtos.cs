namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record ProjectSummaryDto(string Id, string Name, string Description, string Status);

public sealed record ProjectDetailDto(
    string Id,
    string Name,
    string Description,
    string Status,
    int DeveloperCount,
    int TechnologyCount,
    int RepositoryCount,
    int TaskCount);

public sealed record ProjectDeveloperDto(
    string DeveloperId,
    string Name,
    string Role,
    string Since,
    string? OrganizationName);

public sealed record ProjectTechnologyDto(
    string TechnologyId,
    string TechnologyName,
    string Category,
    string? Purpose);

public sealed record ProjectDependencyDto(
    string ProjectId,
    string ProjectName,
    string ProjectStatus,
    int Depth);

public sealed record ProjectTaskDto(
    string TaskId,
    string Title,
    string Status,
    int? Priority);

public sealed record ProjectRepositoryDto(string RepositoryId, string RepositoryName, string Url);

public sealed record RecommendedDeveloperDto(
    string DeveloperId,
    string Name,
    string Role,
    int MatchedSkills,
    int TotalRequired,
    double Coverage,
    string? OrganizationName);

public sealed record IndirectTechnologyDto(
    string TechnologyId,
    string TechnologyName,
    string Category,
    string DependencyProjectId,
    string DependencyProjectName,
    int Depth);

public sealed record ProjectContributorDto(
    string DeveloperId,
    string DeveloperName,
    string Role,
    string RepositoryName,
    int ContributionCount,
    string Since);