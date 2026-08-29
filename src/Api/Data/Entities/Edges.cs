namespace DeveloperKnowledgeGraph.Api.Data.Entities;

public sealed class WorksForEdge
{
    public string DeveloperId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string Since { get; set; } = string.Empty;
}

public sealed class OwnsEdge
{
    public string OrganizationId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

public sealed class WorksOnEdge
{
    public string DeveloperId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Since { get; set; } = string.Empty;
}

public sealed class UsesEdge
{
    public string ProjectId { get; set; } = string.Empty;
    public string TechnologyId { get; set; } = string.Empty;
    public string? Purpose { get; set; }
}

public sealed class DependsOnEdge
{
    public string ProjectId { get; set; } = string.Empty;
    public string DependencyProjectId { get; set; } = string.Empty;
}

public sealed class HasSkillEdge
{
    public string DeveloperId { get; set; } = string.Empty;
    public string TechnologyId { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
    public string Since { get; set; } = string.Empty;
}

public sealed class ContributedToEdge
{
    public string DeveloperId { get; set; } = string.Empty;
    public string RepositoryId { get; set; } = string.Empty;
    public int ContributionCount { get; set; }
    public string Since { get; set; } = string.Empty;
}

public sealed class RequiresSkillEdge
{
    public string TaskId { get; set; } = string.Empty;
    public string TechnologyId { get; set; } = string.Empty;
}