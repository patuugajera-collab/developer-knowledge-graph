using DeveloperKnowledgeGraph.Api.Data;
using DeveloperKnowledgeGraph.Api.Data.Entities;
using DeveloperKnowledgeGraph.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeveloperKnowledgeGraph.Api.Repositories;

/// <summary>
/// SQL (EF Core) implementation of the graph repository. Node types live in
/// their own tables; every relationship type is a join-table carrying its own
/// edge properties. Path-based queries (dependency chains, neighbourhood
/// graphs, shortest paths) run as in-memory breadth-first traversals over the
/// edge tables, which keeps the semantics identical to the old Cypher queries
/// on this small graph.
/// </summary>
public sealed class GraphRepository : IGraphRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<GraphRepository> _logger;

    public GraphRepository(AppDbContext db, ILogger<GraphRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(int Total, IReadOnlyList<DeveloperSummaryDto> Items)> SearchDevelopersAsync(
        string search, int skip, int limit, CancellationToken ct)
    {
        var filtered = _db.Developers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(d =>
                d.Name.Contains(search) || d.Email.Contains(search) || d.Role.Contains(search));
        }

        var total = await filtered.CountAsync(ct);

        var rows = await (
            from d in filtered
            join w in _db.WorksForRelations on d.Id equals w.DeveloperId into wg
            from w in wg.DefaultIfEmpty()
            join o in _db.Organizations on w.OrganizationId equals o.Id into og
            from o in og.DefaultIfEmpty()
            orderby d.Name
            select new { d.Id, d.Name, d.Email, d.Role, OrgName = (string?)o.Name })
            .Skip(skip)
            .Take(limit)
            .ToListAsync(ct);

        var items = rows
            .Select(r => new DeveloperSummaryDto(r.Id, r.Name, r.Email, r.Role, r.OrgName))
            .ToList();

        return (total, items);
    }

    public async Task<(int Total, IReadOnlyList<ProjectSummaryDto> Items)> SearchProjectsAsync(
        string search, string? status, int skip, int limit, CancellationToken ct)
    {
        var filtered = _db.Projects.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        var total = await filtered.CountAsync(ct);

        var rows = await filtered
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(limit)
            .Select(p => new { p.Id, p.Name, p.Description, p.Status })
            .ToListAsync(ct);

        var items = rows
            .Select(r => new ProjectSummaryDto(r.Id, r.Name, r.Description, r.Status))
            .ToList();

        return (total, items);
    }

    public async Task<(int Total, IReadOnlyList<TechnologySummaryDto> Items)> SearchTechnologiesAsync(
        string search, string? category, int skip, int limit, CancellationToken ct)
    {
        var filtered = _db.Technologies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            filtered = filtered.Where(t => t.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(t => t.Name.Contains(search) || t.Category.Contains(search));
        }

        var total = await filtered.CountAsync(ct);

        var rows = await filtered
            .OrderBy(t => t.Name)
            .Skip(skip)
            .Take(limit)
            .Select(t => new { t.Id, t.Name, t.Category })
            .ToListAsync(ct);

        var items = rows
            .Select(r => new TechnologySummaryDto(r.Id, r.Name, r.Category))
            .ToList();

        return (total, items);
    }

    public async Task<DeveloperDetailDto?> GetDeveloperByIdAsync(string id, CancellationToken ct)
    {
        var developer = await _db.Developers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
        if (developer is null)
        {
            return null;
        }

        var projectCount = await _db.WorksOnRelations.CountAsync(w => w.DeveloperId == id, ct);
        var skillCount = await _db.HasSkillRelations.CountAsync(w => w.DeveloperId == id, ct);
        var repositoryCount = await _db.ContributedToRelations.CountAsync(w => w.DeveloperId == id, ct);
        var organizationName = await (
            from o in _db.Organizations.AsNoTracking()
            join w in _db.WorksForRelations on o.Id equals w.OrganizationId
            where w.DeveloperId == id
            select o.Name).FirstOrDefaultAsync();

        return new DeveloperDetailDto(
            developer.Id,
            developer.Name,
            developer.Email,
            developer.Role,
            organizationName,
            projectCount,
            skillCount,
            repositoryCount);
    }

    public async Task<IReadOnlyList<DeveloperProjectDto>> GetDeveloperProjectsAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from w in _db.WorksOnRelations.AsNoTracking()
            join p in _db.Projects on w.ProjectId equals p.Id
            where w.DeveloperId == id
            orderby w.Since descending
            select new { w.Role, w.Since, p.Id, p.Name, p.Status })
            .ToListAsync(ct);

        return rows
            .Select(r => new DeveloperProjectDto(
                r.Id,
                r.Name,
                r.Status,
                string.IsNullOrEmpty(r.Role) ? "Member" : r.Role,
                r.Since))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperSkillDto>> GetDeveloperSkillsAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from h in _db.HasSkillRelations.AsNoTracking()
            join t in _db.Technologies on h.TechnologyId equals t.Id
            where h.DeveloperId == id
            orderby t.Name
            select new { h.Proficiency, h.Since, t.Id, t.Name, t.Category })
            .ToListAsync(ct);

        return rows
            .Select(r => new DeveloperSkillDto(
                r.Id,
                r.Name,
                r.Category,
                string.IsNullOrEmpty(r.Proficiency) ? "Intermediate" : r.Proficiency,
                r.Since))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperRepositoryDto>> GetDeveloperRepositoriesAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from c in _db.ContributedToRelations.AsNoTracking()
            join r in _db.Repositories on c.RepositoryId equals r.Id
            where c.DeveloperId == id
            orderby c.ContributionCount descending
            select new { c.ContributionCount, c.Since, r.Id, r.Name, r.Url })
            .ToListAsync(ct);

        return rows
            .Select(r => new DeveloperRepositoryDto(r.Id, r.Name, r.Url, r.ContributionCount, r.Since))
            .ToList();
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(string id, CancellationToken ct)
    {
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
        {
            return null;
        }

        var developerCount = await _db.WorksOnRelations.CountAsync(w => w.ProjectId == id, ct);
        var technologyCount = await _db.UsesRelations.CountAsync(u => u.ProjectId == id, ct);
        var repositoryCount = await _db.Repositories.CountAsync(r => r.ProjectId == id, ct);
        var taskCount = await _db.Tasks.CountAsync(t => t.ProjectId == id, ct);

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            developerCount,
            technologyCount,
            repositoryCount,
            taskCount);
    }

    public async Task<IReadOnlyList<ProjectDependencyDto>> GetProjectDependenciesAsync(string id, int maxDepth, CancellationToken ct)
    {
        var edges = await _db.DependsOnRelations.AsNoTracking()
            .Select(e => new DependencyRow(e.ProjectId, e.DependencyProjectId))
            .ToListAsync(ct);

        var minDepth = BfsDependencyDepths(edges, id, maxDepth);
        if (minDepth.Count == 0)
        {
            return Array.Empty<ProjectDependencyDto>();
        }

        var projectIds = minDepth.Keys.ToList();
        var projects = await _db.Projects.AsNoTracking()
            .Where(p => projectIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return minDepth
            .Where(kvp => projects.TryGetValue(kvp.Key, out _))
            .Select(kvp => new ProjectDependencyDto(
                kvp.Key,
                projects[kvp.Key].Name,
                projects[kvp.Key].Status,
                kvp.Value))
            .OrderBy(d => d.Depth)
            .ThenBy(d => d.ProjectName)
            .ToArray();
    }

    public async Task<IReadOnlyList<ProjectTechnologyDto>> GetProjectTechnologiesAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from u in _db.UsesRelations.AsNoTracking()
            join t in _db.Technologies on u.TechnologyId equals t.Id
            where u.ProjectId == id
            orderby t.Name
            select new { u.Purpose, t.Id, t.Name, t.Category })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectTechnologyDto(r.Id, r.Name, r.Category, string.IsNullOrEmpty(r.Purpose) ? null : r.Purpose))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectDeveloperDto>> GetProjectDevelopersAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from w in _db.WorksOnRelations.AsNoTracking()
            join d in _db.Developers on w.DeveloperId equals d.Id
            where w.ProjectId == id
            join wf in _db.WorksForRelations on d.Id equals wf.DeveloperId into wfg
            from wf in wfg.DefaultIfEmpty()
            join o in _db.Organizations on wf.OrganizationId equals o.Id into og
            from o in og.DefaultIfEmpty()
            orderby d.Name
            select new { w.Role, w.Since, d.Id, d.Name, OrgName = (string?)o.Name })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectDeveloperDto(
                r.Id,
                r.Name,
                string.IsNullOrEmpty(r.Role) ? "Member" : r.Role,
                r.Since,
                r.OrgName))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectRepositoryDto>> GetProjectRepositoriesAsync(string id, CancellationToken ct)
    {
        var rows = await _db.Repositories.AsNoTracking()
            .Where(r => r.ProjectId == id)
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name, r.Url })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectRepositoryDto(r.Id, r.Name, r.Url))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(string id, CancellationToken ct)
    {
        var rows = await _db.Tasks.AsNoTracking()
            .Where(t => t.ProjectId == id)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.Priority)
            .Select(t => new { t.Id, t.Title, t.Status, t.Priority })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectTaskDto(r.Id, r.Title, r.Status, r.Priority > 0 ? r.Priority : null))
            .ToList();
    }

    public async Task<IReadOnlyList<RecommendedDeveloperDto>> GetRecommendedDevelopersAsync(string id, CancellationToken ct)
    {
        var requiredTechIds = await (
            from t in _db.Tasks.AsNoTracking()
            join req in _db.RequiresSkillRelations on t.Id equals req.TaskId
            where t.ProjectId == id
            select req.TechnologyId)
            .Distinct()
            .ToListAsync(ct);

        if (requiredTechIds.Count == 0)
        {
            return Array.Empty<RecommendedDeveloperDto>();
        }

        var requiredSet = new HashSet<string>(requiredTechIds, StringComparer.Ordinal);
        var totalRequired = requiredTechIds.Count;

        var devRows = await (
            from d in _db.Developers.AsNoTracking()
            join h in _db.HasSkillRelations on d.Id equals h.DeveloperId
            where requiredSet.Contains(h.TechnologyId)
            select new { d.Id, d.Name, d.Role, h.TechnologyId })
            .ToListAsync(ct);

        var orgByDeveloper = (await (
            from wf in _db.WorksForRelations.AsNoTracking()
            join o in _db.Organizations on wf.OrganizationId equals o.Id
            select new OrgNameRow(wf.DeveloperId, o.Name)).ToListAsync(ct))
            .ToDictionary(x => x.DeveloperId, x => x.Name);

        return devRows
            .GroupBy(x => x.Id)
            .Select(g =>
            {
                var first = g.First();
                var matched = g.Select(x => x.TechnologyId).Distinct().Count();
                return new RecommendedDeveloperDto(
                    g.Key,
                    first.Name,
                    first.Role,
                    matched,
                    totalRequired,
                    Math.Round(matched / (double)totalRequired, 2),
                    orgByDeveloper.GetValueOrDefault(g.Key));
            })
            .OrderByDescending(x => x.Coverage)
            .ThenByDescending(x => x.MatchedSkills)
            .ThenBy(x => x.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<IndirectTechnologyDto>> GetProjectIndirectTechnologiesAsync(string id, int maxDepth, CancellationToken ct)
    {
        var edges = await _db.DependsOnRelations.AsNoTracking()
            .Select(e => new DependencyRow(e.ProjectId, e.DependencyProjectId))
            .ToListAsync(ct);

        var minDepth = BfsDependencyDepths(edges, id, maxDepth);
        if (minDepth.Count == 0)
        {
            return Array.Empty<IndirectTechnologyDto>();
        }

        var directTech = await _db.UsesRelations.AsNoTracking()
            .Where(u => u.ProjectId == id)
            .Select(u => u.TechnologyId)
            .ToHashSetAsync(ct);

        var dependencyIds = minDepth.Keys.ToList();
        var projectNames = await _db.Projects.AsNoTracking()
            .Where(p => dependencyIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var rows = await (
            from u in _db.UsesRelations.AsNoTracking()
            join t in _db.Technologies on u.TechnologyId equals t.Id
            where dependencyIds.Contains(u.ProjectId)
            select new { u.ProjectId, t.Id, t.Name, t.Category })
            .ToListAsync(ct);

        var directTechSet = new HashSet<string>(directTech, StringComparer.Ordinal);
        return rows
            .Where(r => !directTechSet.Contains(r.Id))
            .Select(r => new IndirectTechnologyDto(
                r.Id,
                r.Name,
                r.Category,
                r.ProjectId,
                projectNames.GetValueOrDefault(r.ProjectId, r.ProjectId),
                minDepth[r.ProjectId] + 1))
            .OrderBy(x => x.TechnologyName)
            .ThenBy(x => x.Depth)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectContributorDto>> GetProjectContributorsAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from c in _db.ContributedToRelations.AsNoTracking()
            join r in _db.Repositories on c.RepositoryId equals r.Id
            join d in _db.Developers on c.DeveloperId equals d.Id
            where r.ProjectId == id
            orderby d.Name, r.Name
            select new { c.ContributionCount, c.Since, DevId = d.Id, DevName = d.Name, d.Role, RepoName = r.Name })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectContributorDto(
                r.DevId,
                r.DevName,
                r.Role,
                r.RepoName,
                r.ContributionCount,
                r.Since))
            .ToList();
    }

    public async Task<TechnologyDetailDto?> GetTechnologyByIdAsync(string id, CancellationToken ct)
    {
        var technology = await _db.Technologies.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (technology is null)
        {
            return null;
        }

        var projectCount = await _db.UsesRelations.CountAsync(u => u.TechnologyId == id, ct);
        var developerCount = await _db.HasSkillRelations.CountAsync(h => h.TechnologyId == id, ct);

        return new TechnologyDetailDto(technology.Id, technology.Name, technology.Category, projectCount, developerCount);
    }

    public async Task<IReadOnlyList<TechnologyDeveloperDto>> GetTechnologyDevelopersAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from h in _db.HasSkillRelations.AsNoTracking()
            join d in _db.Developers on h.DeveloperId equals d.Id
            where h.TechnologyId == id
            orderby d.Name
            select new { h.Proficiency, h.Since, d.Id, d.Name, d.Role })
            .ToListAsync(ct);

        return rows
            .Select(r => new TechnologyDeveloperDto(
                r.Id,
                r.Name,
                r.Role,
                string.IsNullOrEmpty(r.Proficiency) ? "Intermediate" : r.Proficiency,
                r.Since))
            .ToList();
    }

    public async Task<IReadOnlyList<TechnologyProjectDto>> GetTechnologyProjectsAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from u in _db.UsesRelations.AsNoTracking()
            join p in _db.Projects on u.ProjectId equals p.Id
            where u.TechnologyId == id
            orderby p.Name
            select new { u.Purpose, p.Id, p.Name, p.Status })
            .ToListAsync(ct);

        return rows
            .Select(r => new TechnologyProjectDto(r.Id, r.Name, r.Status, string.IsNullOrEmpty(r.Purpose) ? null : r.Purpose))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetTechnologyCategoriesAsync(CancellationToken ct)
    {
        return await _db.Technologies.AsNoTracking()
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct)
    {
        var organizations = await _db.Organizations.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new { o.Id, o.Name })
            .ToListAsync(ct);

        var developerCounts = await _db.WorksForRelations.AsNoTracking()
            .GroupBy(w => w.OrganizationId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var projectCounts = await _db.OwnsRelations.AsNoTracking()
            .GroupBy(w => w.OrganizationId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var devLookup = developerCounts.ToDictionary(x => x.OrgId, x => x.Count);
        var projLookup = projectCounts.ToDictionary(x => x.OrgId, x => x.Count);

        return organizations
            .Select(o => new OrganizationSummaryDto(
                o.Id,
                o.Name,
                devLookup.GetValueOrDefault(o.Id),
                projLookup.GetValueOrDefault(o.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperSummaryDto>> GetOrganizationDevelopersAsync(string id, CancellationToken ct)
    {
        var rows = await (
            from w in _db.WorksForRelations.AsNoTracking()
            join d in _db.Developers on w.DeveloperId equals d.Id
            where w.OrganizationId == id
            orderby d.Name
            select new { d.Id, d.Name, d.Email, d.Role })
            .ToListAsync(ct);

        return rows
            .Select(r => new DeveloperSummaryDto(r.Id, r.Name, r.Email, r.Role, null))
            .ToList();
    }

    public async Task<DashboardCounts> GetDashboardCountsAsync(CancellationToken ct)
    {
        var developers = await _db.Developers.CountAsync(ct);
        var projects = await _db.Projects.CountAsync(ct);
        var technologies = await _db.Technologies.CountAsync(ct);
        var repositories = await _db.Repositories.CountAsync(ct);
        var tasks = await _db.Tasks.CountAsync(ct);
        var organizations = await _db.Organizations.CountAsync(ct);

        var worksFor = await _db.WorksForRelations.CountAsync(ct);
        var owns = await _db.OwnsRelations.CountAsync(ct);
        var worksOn = await _db.WorksOnRelations.CountAsync(ct);
        var uses = await _db.UsesRelations.CountAsync(ct);
        var dependsOn = await _db.DependsOnRelations.CountAsync(ct);
        var hasSkill = await _db.HasSkillRelations.CountAsync(ct);
        var contributedTo = await _db.ContributedToRelations.CountAsync(ct);
        var requiresSkill = await _db.RequiresSkillRelations.CountAsync(ct);

        var relationships = worksFor + owns + worksOn + uses + dependsOn + hasSkill
            + contributedTo + requiresSkill + repositories + tasks;
        var developerConnections = worksFor + worksOn + hasSkill + contributedTo;

        var averageConnections = developers == 0
            ? 0.0
            : Math.Round(developerConnections / (double)developers, 2);

        return new DashboardCounts(
            developers,
            projects,
            technologies,
            repositories,
            tasks,
            organizations,
            relationships,
            averageConnections);
    }

    public async Task<IReadOnlyList<ProjectStatusCountDto>> GetProjectStatusCountsAsync(CancellationToken ct)
    {
        var rows = await _db.Projects.AsNoTracking()
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .Select(r => new ProjectStatusCountDto(r.Status, r.Count))
            .OrderBy(x => x.Status)
            .ToList();
    }

    public async Task<IReadOnlyList<RelationshipTypeCountDto>> GetRelationshipTypeCountsAsync(CancellationToken ct)
    {
        var worksFor = await _db.WorksForRelations.CountAsync(ct);
        var owns = await _db.OwnsRelations.CountAsync(ct);
        var worksOn = await _db.WorksOnRelations.CountAsync(ct);
        var uses = await _db.UsesRelations.CountAsync(ct);
        var dependsOn = await _db.DependsOnRelations.CountAsync(ct);
        var hasSkill = await _db.HasSkillRelations.CountAsync(ct);
        var contributedTo = await _db.ContributedToRelations.CountAsync(ct);
        var requiresSkill = await _db.RequiresSkillRelations.CountAsync(ct);
        var belongsTo = await _db.Repositories.CountAsync(ct);
        var hasTask = await _db.Tasks.CountAsync(ct);

        return new[]
            {
                new RelationshipTypeCountDto("WORKS_FOR", worksFor),
                new RelationshipTypeCountDto("OWNS", owns),
                new RelationshipTypeCountDto("WORKS_ON", worksOn),
                new RelationshipTypeCountDto("USES", uses),
                new RelationshipTypeCountDto("DEPENDS_ON", dependsOn),
                new RelationshipTypeCountDto("HAS_SKILL", hasSkill),
                new RelationshipTypeCountDto("CONTRIBUTED_TO", contributedTo),
                new RelationshipTypeCountDto("REQUIRES_SKILL", requiresSkill),
                new RelationshipTypeCountDto("BELONGS_TO", belongsTo),
                new RelationshipTypeCountDto("HAS_TASK", hasTask),
            }
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Type)
            .ToList();
    }

    public async Task<IReadOnlyList<CentralTechnologyDto>> GetCentralTechnologiesAsync(int limit, CancellationToken ct)
    {
        var usageRows = await (
            from u in _db.UsesRelations.AsNoTracking()
            join t in _db.Technologies on u.TechnologyId equals t.Id
            group u by new { t.Id, t.Name, t.Category } into g
            select new { g.Key.Id, g.Key.Name, g.Key.Category, ProjectUsage = g.Count() })
            .ToListAsync(ct);

        var skillCounts = await _db.HasSkillRelations.AsNoTracking()
            .GroupBy(h => h.TechnologyId)
            .Select(g => new { TechId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TechId, x => x.Count, ct);

        return usageRows
            .Select(r =>
            {
                var skillCount = skillCounts.GetValueOrDefault(r.Id);
                return new CentralTechnologyDto(r.Id, r.Name, r.Category, r.ProjectUsage, skillCount, r.ProjectUsage + skillCount);
            })
            .OrderByDescending(x => x.Centrality)
            .ThenByDescending(x => x.ProjectUsage)
            .ThenBy(x => x.Name)
            .Take(limit)
            .ToList();
    }

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<SearchResultDto>();
        }

        var developers = await _db.Developers.AsNoTracking()
            .Where(d => d.Name.Contains(query))
            .Select(d => new { d.Id, d.Name, Subtitle = d.Role })
            .ToListAsync(ct);

        var projects = await _db.Projects.AsNoTracking()
            .Where(p => p.Name.Contains(query))
            .Select(p => new { p.Id, p.Name, Subtitle = p.Status })
            .ToListAsync(ct);

        var technologies = await _db.Technologies.AsNoTracking()
            .Where(t => t.Name.Contains(query))
            .Select(t => new { t.Id, t.Name, Subtitle = t.Category })
            .ToListAsync(ct);

        var repositories = await _db.Repositories.AsNoTracking()
            .Where(r => r.Name.Contains(query))
            .Select(r => new { r.Id, r.Name, Subtitle = r.Url })
            .ToListAsync(ct);

        var results = new List<SearchResultDto>(developers.Count + projects.Count + technologies.Count + repositories.Count);
        foreach (var d in developers)
        {
            results.Add(new SearchResultDto(d.Id, d.Name, "Developer", d.Subtitle));
        }

        foreach (var p in projects)
        {
            results.Add(new SearchResultDto(p.Id, p.Name, "Project", p.Subtitle));
        }

        foreach (var t in technologies)
        {
            results.Add(new SearchResultDto(t.Id, t.Name, "Technology", t.Subtitle));
        }

        foreach (var r in repositories)
        {
            results.Add(new SearchResultDto(r.Id, r.Name, "Repository", r.Subtitle));
        }

        return results.OrderBy(x => x.Name).Take(limit).ToList();
    }

    public async Task<GraphResponseDto?> GetGraphAsync(string id, int maxDepth, CancellationToken ct)
    {
        var (nodes, edges) = await BuildGraphAsync(ct);
        var startEntry = nodes.FirstOrDefault(kvp => kvp.Value.Id == id);
        if (startEntry.Value is null)
        {
            return null;
        }

        var startKey = startEntry.Key;
        var depth = new Dictionary<string, int>(StringComparer.Ordinal) { [startKey] = 0 };
        var queue = new Queue<string>();
        queue.Enqueue(startKey);

        var resultNodes = new List<GraphNodeDto>();
        var resultEdges = new HashSet<GraphEdgeInfo>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDepth = depth[current];
            resultNodes.Add(ToGraphNodeDto(nodes[current]));

            if (currentDepth >= maxDepth)
            {
                continue;
            }

            foreach (var edge in edges)
            {
                if (edge.SourceKey != current && edge.TargetKey != current)
                {
                    continue;
                }

                var otherKey = edge.SourceKey == current ? edge.TargetKey : edge.SourceKey;
                if (!nodes.ContainsKey(otherKey))
                {
                    continue;
                }

                if (depth.TryGetValue(otherKey, out var otherDepth))
                {
                    if (Math.Min(currentDepth, otherDepth) < maxDepth)
                    {
                        resultEdges.Add(edge);
                    }

                    continue;
                }

                depth[otherKey] = currentDepth + 1;
                queue.Enqueue(otherKey);
                resultEdges.Add(edge);
            }
        }

        var edgeDtos = resultEdges.Select(ToGraphEdgeDto).ToList();
        return new GraphResponseDto(resultNodes, edgeDtos, id, maxDepth);
    }

    public async Task<ShortestPathDto?> GetShortestPathAsync(string developerId, string projectId, CancellationToken ct)
    {
        var startKey = KeyOf("Developer", developerId);
        var goalKey = KeyOf("Project", projectId);

        var (nodes, edges) = await BuildGraphAsync(ct);
        if (!nodes.ContainsKey(startKey) || !nodes.ContainsKey(goalKey))
        {
            return null;
        }

        var visited = new Dictionary<string, (string Previous, string EdgeType)>(StringComparer.Ordinal)
        {
            [startKey] = (string.Empty, string.Empty),
        };
        var queue = new Queue<string>();
        queue.Enqueue(startKey);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goalKey)
            {
                break;
            }

            foreach (var edge in edges)
            {
                if (edge.SourceKey != current && edge.TargetKey != current)
                {
                    continue;
                }

                var otherKey = edge.SourceKey == current ? edge.TargetKey : edge.SourceKey;
                if (visited.ContainsKey(otherKey))
                {
                    continue;
                }

                visited[otherKey] = (current, edge.Type);
                queue.Enqueue(otherKey);

                if (otherKey == goalKey)
                {
                    break;
                }
            }
        }

        if (!visited.ContainsKey(goalKey))
        {
            return null;
        }

        var keys = new List<string>();
        var edgeTypes = new List<string>();
        var cursor = goalKey;
        while (cursor != startKey)
        {
            keys.Add(cursor);
            var (previous, edgeType) = visited[cursor];
            edgeTypes.Add(edgeType);
            cursor = previous;
        }

        keys.Add(startKey);
        keys.Reverse();
        edgeTypes.Reverse();

        var developer = nodes[startKey];
        var project = nodes[goalKey];

        var steps = new List<PathStepDto> { new(developer.Type, developer.Id, developer.Name, null) };
        for (var i = 1; i < keys.Count; i++)
        {
            var node = nodes[keys[i]];
            steps.Add(new PathStepDto(node.Type, node.Id, node.Name, edgeTypes[i - 1]));
        }

        return new ShortestPathDto(developer.Id, developer.Name, project.Id, project.Name, steps, steps.Count - 1);
    }

    public async Task<bool> NodeExistsAsync(string label, string id, CancellationToken ct)
    {
        return label switch
        {
            "Developer" => await _db.Developers.AnyAsync(d => d.Id == id, ct),
            "Project" => await _db.Projects.AnyAsync(p => p.Id == id, ct),
            "Technology" => await _db.Technologies.AnyAsync(t => t.Id == id, ct),
            "Organization" => await _db.Organizations.AnyAsync(o => o.Id == id, ct),
            "Repository" => await _db.Repositories.AnyAsync(r => r.Id == id, ct),
            "Task" => await _db.Tasks.AnyAsync(t => t.Id == id, ct),
            _ => false,
        };
    }

    // ---- Internal helpers -------------------------------------------------

    private sealed record GraphNodeInfo(
        string Type,
        string Id,
        string Name,
        string Key,
        IReadOnlyDictionary<string, object?> Properties);

    private sealed record GraphEdgeInfo(
        string SourceKey,
        string TargetKey,
        string SourceId,
        string TargetId,
        string Type,
        IReadOnlyDictionary<string, object?> Properties);

    private sealed record OrgNameRow(string DeveloperId, string Name);

    private sealed record DependencyRow(string ProjectId, string DependencyProjectId);

    private static string KeyOf(string type, string id) => type + "\u0001" + id;

    private async Task<(Dictionary<string, GraphNodeInfo> Nodes, List<GraphEdgeInfo> Edges)> BuildGraphAsync(CancellationToken ct)
    {
        var nodes = new Dictionary<string, GraphNodeInfo>(StringComparer.Ordinal);
        var edges = new List<GraphEdgeInfo>();

        var organizations = await _db.Organizations.AsNoTracking().ToListAsync(ct);
        foreach (var o in organizations)
        {
            nodes[KeyOf("Organization", o.Id)] = new GraphNodeInfo("Organization", o.Id, o.Name, KeyOf("Organization", o.Id), Props(("name", o.Name)));
        }

        var developers = await _db.Developers.AsNoTracking().ToListAsync(ct);
        foreach (var d in developers)
        {
            nodes[KeyOf("Developer", d.Id)] = new GraphNodeInfo(
                "Developer", d.Id, d.Name, KeyOf("Developer", d.Id),
                Props(("name", d.Name), ("email", d.Email), ("role", d.Role)));
        }

        var projects = await _db.Projects.AsNoTracking().ToListAsync(ct);
        foreach (var p in projects)
        {
            nodes[KeyOf("Project", p.Id)] = new GraphNodeInfo(
                "Project", p.Id, p.Name, KeyOf("Project", p.Id),
                Props(("name", p.Name), ("description", p.Description), ("status", p.Status)));
        }

        var technologies = await _db.Technologies.AsNoTracking().ToListAsync(ct);
        foreach (var t in technologies)
        {
            nodes[KeyOf("Technology", t.Id)] = new GraphNodeInfo(
                "Technology", t.Id, t.Name, KeyOf("Technology", t.Id),
                Props(("name", t.Name), ("category", t.Category)));
        }

        var repositories = await _db.Repositories.AsNoTracking().ToListAsync(ct);
        foreach (var r in repositories)
        {
            nodes[KeyOf("Repository", r.Id)] = new GraphNodeInfo(
                "Repository", r.Id, r.Name, KeyOf("Repository", r.Id),
                Props(("name", r.Name), ("url", r.Url)));
        }

        var tasks = await _db.Tasks.AsNoTracking().ToListAsync(ct);
        foreach (var t in tasks)
        {
            nodes[KeyOf("Task", t.Id)] = new GraphNodeInfo(
                "Task", t.Id, t.Title, KeyOf("Task", t.Id),
                Props(("title", t.Title), ("status", t.Status), ("priority", t.Priority)));
        }

        var worksFor = await _db.WorksForRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in worksFor)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Developer", e.DeveloperId),
                KeyOf("Organization", e.OrganizationId),
                e.DeveloperId,
                e.OrganizationId,
                "WORKS_FOR",
                Props(("since", e.Since))));
        }

        var owns = await _db.OwnsRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in owns)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Organization", e.OrganizationId),
                KeyOf("Project", e.ProjectId),
                e.OrganizationId,
                e.ProjectId,
                "OWNS",
                Props()));
        }

        var worksOn = await _db.WorksOnRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in worksOn)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Developer", e.DeveloperId),
                KeyOf("Project", e.ProjectId),
                e.DeveloperId,
                e.ProjectId,
                "WORKS_ON",
                Props(("role", e.Role), ("since", e.Since))));
        }

        var uses = await _db.UsesRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in uses)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Project", e.ProjectId),
                KeyOf("Technology", e.TechnologyId),
                e.ProjectId,
                e.TechnologyId,
                "USES",
                Props(("purpose", e.Purpose))));
        }

        var dependsOn = await _db.DependsOnRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in dependsOn)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Project", e.ProjectId),
                KeyOf("Project", e.DependencyProjectId),
                e.ProjectId,
                e.DependencyProjectId,
                "DEPENDS_ON",
                Props()));
        }

        var hasSkill = await _db.HasSkillRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in hasSkill)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Developer", e.DeveloperId),
                KeyOf("Technology", e.TechnologyId),
                e.DeveloperId,
                e.TechnologyId,
                "HAS_SKILL",
                Props(("proficiency", e.Proficiency), ("since", e.Since))));
        }

        var contributedTo = await _db.ContributedToRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in contributedTo)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Developer", e.DeveloperId),
                KeyOf("Repository", e.RepositoryId),
                e.DeveloperId,
                e.RepositoryId,
                "CONTRIBUTED_TO",
                Props(("contributionCount", e.ContributionCount), ("since", e.Since))));
        }

        var requiresSkill = await _db.RequiresSkillRelations.AsNoTracking().ToListAsync(ct);
        foreach (var e in requiresSkill)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Task", e.TaskId),
                KeyOf("Technology", e.TechnologyId),
                e.TaskId,
                e.TechnologyId,
                "REQUIRES_SKILL",
                Props()));
        }

        foreach (var t in tasks)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Project", t.ProjectId),
                KeyOf("Task", t.Id),
                t.ProjectId,
                t.Id,
                "HAS_TASK",
                Props()));
        }

        foreach (var r in repositories)
        {
            edges.Add(new GraphEdgeInfo(
                KeyOf("Repository", r.Id),
                KeyOf("Project", r.ProjectId),
                r.Id,
                r.ProjectId,
                "BELONGS_TO",
                Props()));
        }

        return (nodes, edges);
    }

    private static IReadOnlyDictionary<string, object?> Props(params (string Key, object? Value)[] items)
    {
        var dictionary = new Dictionary<string, object?>(items.Length);
        foreach (var (key, value) in items)
        {
            dictionary[key] = value;
        }

        return dictionary;
    }

    private static GraphNodeDto ToGraphNodeDto(GraphNodeInfo node)
    {
        return new GraphNodeDto(node.Id, node.Name, node.Type, node.Properties);
    }

    private static GraphEdgeDto ToGraphEdgeDto(GraphEdgeInfo edge)
    {
        return new GraphEdgeDto(
            $"{edge.SourceId}\u0001{edge.Type}\u0001{edge.TargetId}",
            edge.SourceId,
            edge.TargetId,
            edge.Type,
            edge.Properties);
    }

    private static void OrganizationNamesByDeveloper(List<dynamic> rows, out Dictionary<string, string> byDeveloper)
    {
        _ = rows;
        byDeveloper = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static Dictionary<string, int> BfsDependencyDepths(List<DependencyRow> edges, string rootId, int maxDepth)
    {
        var byProject = edges
            .GroupBy(e => e.ProjectId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.DependencyProjectId).ToList());

        var minDepth = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<(string Id, int Depth)>();

        if (byProject.TryGetValue(rootId, out var directDeps))
        {
            foreach (var dependency in directDeps)
            {
                if (dependency == rootId || minDepth.ContainsKey(dependency))
                {
                    continue;
                }

                minDepth[dependency] = 1;
                queue.Enqueue((dependency, 1));
            }
        }

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth >= maxDepth)
            {
                continue;
            }

            if (!byProject.TryGetValue(current, out var nextDeps))
            {
                continue;
            }

            foreach (var next in nextDeps)
            {
                if (next == current || minDepth.ContainsKey(next))
                {
                    continue;
                }

                minDepth[next] = depth + 1;
                queue.Enqueue((next, depth + 1));
            }
        }

        return minDepth;
    }
}