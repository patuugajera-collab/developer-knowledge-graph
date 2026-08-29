using DeveloperKnowledgeGraph.Api.DTOs;
using Neo4j.Driver;

namespace DeveloperKnowledgeGraph.Api.Repositories;

/// <summary>
/// CognoDB (Neo4j-compatible, Cypher over Bolt) implementation of the graph
/// repository. Node types carry labels and relationships are typed edges with
/// their own properties. Traversals (dependency chains, neighbourhood graphs,
/// shortest paths) run natively in Cypher (variable-length paths, recursive
/// matching) or, for the neighbourhood/shortest-path explorer, over the full
/// edge set loaded once per request and traversed in memory.
/// </summary>
public sealed class GraphRepository : IGraphRepository
{
    private readonly IDriver _driver;
    private readonly ILogger<GraphRepository> _logger;

    public GraphRepository(IDriver driver, ILogger<GraphRepository> logger)
    {
        _driver = driver;
        _logger = logger;
    }

    // ---- Developers -------------------------------------------------------

    public async Task<(int Total, IReadOnlyList<DeveloperSummaryDto> Items)> SearchDevelopersAsync(
        string search, int skip, int limit, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var totalInput = await session.RunAsync(
            """
            MATCH (d:Developer)
            WHERE $search = '' OR d.name CONTAINS $search OR d.email CONTAINS $search OR d.role CONTAINS $search
            RETURN count(d) AS total
            """,
            new { search = search ?? string.Empty });
        var total = (int)await SingleLongAsync(totalInput, "total", ct);

        var cursor = await session.RunAsync(
            """
            MATCH (d:Developer)
            WHERE $search = '' OR d.name CONTAINS $search OR d.email CONTAINS $search OR d.role CONTAINS $search
            OPTIONAL MATCH (d)-[:WORKS_FOR]->(o:Organization)
            RETURN d.id AS id, d.name AS name, d.email AS email, d.role AS role, o.name AS orgName
            ORDER BY d.name
            SKIP $skip LIMIT $limit
            """,
            new { search = search ?? string.Empty, skip = (long)skip, limit = (long)limit });

        var rows = await cursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["email"].As<string>(), r["role"].As<string>(), r["orgName"].As<string?>()), cancellationToken: ct);
        var items = rows
            .Select(r => new DeveloperSummaryDto(r.Item1, r.Item2, r.Item3, r.Item4, r.Item5))
            .ToList();

        return (total, items);
    }

    public async Task<(int Total, IReadOnlyList<ProjectSummaryDto> Items)> SearchProjectsAsync(
        string search, string? status, int skip, int limit, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var totalInput = await session.RunAsync(
            """
            MATCH (p:Project)
            WHERE ($status = '' OR p.status = $status)
              AND ($search = '' OR p.name CONTAINS $search OR p.description CONTAINS $search)
            RETURN count(p) AS total
            """,
            new { search = search ?? string.Empty, status = status ?? string.Empty });
        var total = (int)await SingleLongAsync(totalInput, "total", ct);

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project)
            WHERE ($status = '' OR p.status = $status)
              AND ($search = '' OR p.name CONTAINS $search OR p.description CONTAINS $search)
            RETURN p.id AS id, p.name AS name, p.description AS description, p.status AS status
            ORDER BY p.name
            SKIP $skip LIMIT $limit
            """,
            new { search = search ?? string.Empty, status = status ?? string.Empty, skip = (long)skip, limit = (long)limit });

        var rows = await cursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["description"].As<string>(), r["status"].As<string>()), cancellationToken: ct);
        var items = rows
            .Select(r => new ProjectSummaryDto(r.Item1, r.Item2, r.Item3, r.Item4))
            .ToList();

        return (total, items);
    }

    public async Task<(int Total, IReadOnlyList<TechnologySummaryDto> Items)> SearchTechnologiesAsync(
        string search, string? category, int skip, int limit, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var totalInput = await session.RunAsync(
            """
            MATCH (t:Technology)
            WHERE ($category = '' OR t.category = $category)
              AND ($search = '' OR t.name CONTAINS $search OR t.category CONTAINS $search)
            RETURN count(t) AS total
            """,
            new { search = search ?? string.Empty, category = category ?? string.Empty });
        var total = (int)await SingleLongAsync(totalInput, "total", ct);

        var cursor = await session.RunAsync(
            """
            MATCH (t:Technology)
            WHERE ($category = '' OR t.category = $category)
              AND ($search = '' OR t.name CONTAINS $search OR t.category CONTAINS $search)
            RETURN t.id AS id, t.name AS name, t.category AS category
            ORDER BY t.name
            SKIP $skip LIMIT $limit
            """,
            new { search = search ?? string.Empty, category = category ?? string.Empty, skip = (long)skip, limit = (long)limit });

        var rows = await cursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["category"].As<string>()), cancellationToken: ct);
        var items = rows
            .Select(r => new TechnologySummaryDto(r.Item1, r.Item2, r.Item3))
            .ToList();

        return (total, items);
    }

    public async Task<DeveloperDetailDto?> GetDeveloperByIdAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (d:Developer {id: $id})
            OPTIONAL MATCH (d)-[:WORKS_FOR]->(o:Organization)
            RETURN d.id AS id, d.name AS name, d.email AS email, d.role AS role, o.name AS orgName
            """,
            new { id });

        var row = await TrySingleAsync(cursor, ct);
        if (row is null)
        {
            return null;
        }

        var projectCount = await CountAsync(session, $"MATCH (d:Developer {{id: $id}})-[:WORKS_ON]->(:Project) RETURN count(*) AS c", new { id }, ct);
        var skillCount = await CountAsync(session, $"MATCH (d:Developer {{id: $id}})-[:HAS_SKILL]->(:Technology) RETURN count(*) AS c", new { id }, ct);
        var repositoryCount = await CountAsync(session, $"MATCH (d:Developer {{id: $id}})-[:CONTRIBUTED_TO]->(:Repository) RETURN count(*) AS c", new { id }, ct);

        return new DeveloperDetailDto(
            id,
            row["name"].As<string>(),
            row["email"].As<string>(),
            row["role"].As<string>(),
            row["orgName"].As<string?>(),
            projectCount,
            skillCount,
            repositoryCount);
    }

    public async Task<IReadOnlyList<DeveloperProjectDto>> GetDeveloperProjectsAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (d:Developer {id: $id})-[r:WORKS_ON]->(p:Project)
            RETURN p.id AS projectId, p.name AS projectName, p.status AS status,
                   r.role AS role, r.since AS since
            ORDER BY r.since DESC
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["projectId"].As<string>(), r["projectName"].As<string>(), r["status"].As<string>(), r["role"].As<string?>(), r["since"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new DeveloperProjectDto(
                r.Item1,
                r.Item2,
                r.Item3,
                string.IsNullOrEmpty(r.Item4) ? "Member" : r.Item4,
                r.Item5 ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperSkillDto>> GetDeveloperSkillsAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (d:Developer {id: $id})-[h:HAS_SKILL]->(t:Technology)
            RETURN t.id AS technologyId, t.name AS technologyName, t.category AS category,
                   h.proficiency AS proficiency, h.since AS since
            ORDER BY t.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["technologyId"].As<string>(), r["technologyName"].As<string>(), r["category"].As<string>(), r["proficiency"].As<string?>(), r["since"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new DeveloperSkillDto(
                r.Item1,
                r.Item2,
                r.Item3,
                string.IsNullOrEmpty(r.Item4) ? "Intermediate" : r.Item4,
                r.Item5 ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperRepositoryDto>> GetDeveloperRepositoriesAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (d:Developer {id: $id})-[c:CONTRIBUTED_TO]->(r:Repository)
            RETURN r.id AS repositoryId, r.name AS repositoryName, r.url AS url,
                   c.contributionCount AS contributionCount, c.since AS since
            ORDER BY c.contributionCount DESC
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["repositoryId"].As<string>(), r["repositoryName"].As<string>(), r["url"].As<string>(), r["contributionCount"].As<long>(), r["since"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new DeveloperRepositoryDto(
                r.Item1,
                r.Item2,
                r.Item3,
                (int)r.Item4,
                r.Item5 ?? string.Empty))
            .ToList();
    }

    // ---- Projects ---------------------------------------------------------

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})
            RETURN p.id AS id, p.name AS name, p.description AS description, p.status AS status
            """,
            new { id });

        var row = await TrySingleAsync(cursor, ct);
        if (row is null)
        {
            return null;
        }

        var developerCount = await CountAsync(session, $"MATCH (p:Project {{id: $id}})<-[:WORKS_ON]-(:Developer) RETURN count(*) AS c", new { id }, ct);
        var technologyCount = await CountAsync(session, $"MATCH (p:Project {{id: $id}})-[:USES]->(:Technology) RETURN count(*) AS c", new { id }, ct);
        var repositoryCount = await CountAsync(session, $"MATCH (p:Project {{id: $id}})<-[:BELONGS_TO]-(:Repository) RETURN count(*) AS c", new { id }, ct);
        var taskCount = await CountAsync(session, $"MATCH (p:Project {{id: $id}})-[:HAS_TASK]->(:Task) RETURN count(*) AS c", new { id }, ct);

        return new ProjectDetailDto(
            id,
            row["name"].As<string>(),
            row["description"].As<string>(),
            row["status"].As<string>(),
            developerCount,
            technologyCount,
            repositoryCount,
            taskCount);
    }

    public async Task<IReadOnlyList<ProjectDependencyDto>> GetProjectDependenciesAsync(string id, int maxDepth, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH path = (p:Project {id: $id})-[:DEPENDS_ON*1..$maxDepth]->(dep:Project)
            WITH dep, min(length(path)) AS depth
            RETURN dep.id AS projectId, dep.name AS projectName, dep.status AS status, depth
            ORDER BY depth
            """,
            new { id, maxDepth = (long)maxDepth });

        var rows = await cursor.ToListAsync(r => (r["projectId"].As<string>(), r["projectName"].As<string>(), r["status"].As<string>(), r["depth"].As<long>()), cancellationToken: ct);

        return rows
            .Where(r => !string.Equals(r.Item1, id, StringComparison.Ordinal))
            .Select(r => new ProjectDependencyDto(r.Item1, r.Item2, r.Item3, (int)r.Item4))
            .OrderBy(d => d.Depth)
            .ThenBy(d => d.ProjectName)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectTechnologyDto>> GetProjectTechnologiesAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})-[u:USES]->(t:Technology)
            RETURN t.id AS technologyId, t.name AS technologyName, t.category AS category, u.purpose AS purpose
            ORDER BY t.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["technologyId"].As<string>(), r["technologyName"].As<string>(), r["category"].As<string>(), r["purpose"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectTechnologyDto(r.Item1, r.Item2, r.Item3, string.IsNullOrEmpty(r.Item4) ? null : r.Item4))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectDeveloperDto>> GetProjectDevelopersAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})<-[w:WORKS_ON]-(d:Developer)
            OPTIONAL MATCH (d)-[:WORKS_FOR]->(o:Organization)
            RETURN d.id AS developerId, d.name AS name, w.role AS role, w.since AS since, o.name AS orgName
            ORDER BY d.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["developerId"].As<string>(), r["name"].As<string>(), r["role"].As<string?>(), r["since"].As<string?>(), r["orgName"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectDeveloperDto(
                r.Item1,
                r.Item2,
                string.IsNullOrEmpty(r.Item3) ? "Member" : r.Item3,
                r.Item4 ?? string.Empty,
                r.Item5))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectRepositoryDto>> GetProjectRepositoriesAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})<-[:BELONGS_TO]-(r:Repository)
            RETURN r.id AS repositoryId, r.name AS repositoryName, r.url AS url
            ORDER BY r.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["repositoryId"].As<string>(), r["repositoryName"].As<string>(), r["url"].As<string>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectRepositoryDto(r.Item1, r.Item2, r.Item3))
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})-[:HAS_TASK]->(t:Task)
            RETURN t.id AS taskId, t.title AS title, t.status AS status, t.priority AS priority
            ORDER BY t.status, t.priority
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["taskId"].As<string>(), r["title"].As<string>(), r["status"].As<string>(), r["priority"].As<long?>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectTaskDto(r.Item1, r.Item2, r.Item3, r.Item4 is > 0 ? (int)r.Item4 : null))
            .ToList();
    }

    public async Task<IReadOnlyList<RecommendedDeveloperDto>> GetRecommendedDevelopersAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})-[:HAS_TASK]->(:Task)-[:REQUIRES_SKILL]->(tech:Technology)
            WITH collect(DISTINCT tech.id) AS required
            MATCH (d:Developer)-[h:HAS_SKILL]->(t:Technology)
            WHERE t.id IN required
            OPTIONAL MATCH (d)-[:WORKS_FOR]->(o:Organization)
            WITH required, d, o, collect(DISTINCT t.id) AS matched
            RETURN d.id AS developerId, d.name AS name, d.role AS role,
                   size(matched) AS matchedSkills,
                   size(required) AS totalRequired,
                   o.name AS orgName
            ORDER BY size(matched) DESC, d.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (
            r["developerId"].As<string>(),
            r["name"].As<string>(),
            r["role"].As<string>(),
            r["matchedSkills"].As<long>(),
            r["totalRequired"].As<long>(),
            r["orgName"].As<string?>()), cancellationToken: ct);

        return rows
            .Select(r => new RecommendedDeveloperDto(
                r.Item1,
                r.Item2,
                r.Item3,
                (int)r.Item4,
                (int)r.Item5,
                r.Item5 == 0 ? 0.0 : Math.Round(r.Item4 / (double)r.Item5, 2),
                r.Item6))
            .OrderByDescending(x => x.Coverage)
            .ThenByDescending(x => x.MatchedSkills)
            .ThenBy(x => x.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<IndirectTechnologyDto>> GetProjectIndirectTechnologiesAsync(string id, int maxDepth, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var depCursor = await session.RunAsync(
            """
            MATCH path = (p:Project {id: $id})-[:DEPENDS_ON*1..$maxDepth]->(dep:Project)
            WITH dep.id AS projectId, min(length(path)) AS depth
            RETURN projectId, depth
            """,
            new { id, maxDepth = (long)maxDepth });

        var dependencyRows = await depCursor.ToListAsync(r => (r["projectId"].As<string>(), r["depth"].As<long>()), cancellationToken: ct);
        if (dependencyRows.Count == 0)
        {
            return Array.Empty<IndirectTechnologyDto>();
        }

        var minDepth = dependencyRows
            .GroupBy(r => r.Item1)
            .ToDictionary(g => g.Key, g => (int)g.Min(x => x.Item2), StringComparer.Ordinal);
        var dependencyIds = minDepth.Keys.ToList();

        var directTechCursor = await session.RunAsync(
            "MATCH (p:Project {id: $id})-[:USES]->(t:Technology) RETURN t.id AS techId",
            new { id });
        var directTech = (await directTechCursor.ToListAsync(r => r["techId"].As<string>(), cancellationToken: ct)).ToHashSet(StringComparer.Ordinal);

        var rowsCursor = await session.RunAsync(
            """
            MATCH (p:Project)-[u:USES]->(t:Technology)
            WHERE p.id IN $dependencyIds
            RETURN t.id AS technologyId, t.name AS technologyName, t.category AS category,
                   p.id AS projectId, p.name AS projectName
            """,
            new { dependencyIds });

        var nameLookup = await session.RunAsync(
            "MATCH (p:Project) WHERE p.id IN $dependencyIds RETURN p.id AS id, p.name AS name",
            new { dependencyIds });
        var projectNames = (await nameLookup.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>()), cancellationToken: ct))
            .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal);

        var techRows = await rowsCursor.ToListAsync(r => (
            r["technologyId"].As<string>(),
            r["technologyName"].As<string>(),
            r["category"].As<string>(),
            r["projectId"].As<string>(),
            r["projectName"].As<string>()), cancellationToken: ct);

        return techRows
            .Where(r => !directTech.Contains(r.Item1))
            .Select(r => new IndirectTechnologyDto(
                r.Item1,
                r.Item2,
                r.Item3,
                r.Item4,
                r.Item5,
                minDepth.GetValueOrDefault(r.Item4) + 1))
            .OrderBy(x => x.TechnologyName)
            .ThenBy(x => x.Depth)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectContributorDto>> GetProjectContributorsAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (p:Project {id: $id})<-[:BELONGS_TO]-(r:Repository)<-[c:CONTRIBUTED_TO]-(d:Developer)
            RETURN d.id AS developerId, d.name AS developerName, d.role AS role,
                   r.name AS repositoryName, c.contributionCount AS contributionCount, c.since AS since
            ORDER BY d.name, r.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["developerId"].As<string>(), r["developerName"].As<string>(), r["role"].As<string>(), r["repositoryName"].As<string>(), r["contributionCount"].As<long>(), r["since"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectContributorDto(
                r.Item1,
                r.Item2,
                r.Item3,
                r.Item4,
                (int)r.Item5,
                r.Item6 ?? string.Empty))
            .ToList();
    }

    // ---- Technologies -----------------------------------------------------

    public async Task<TechnologyDetailDto?> GetTechnologyByIdAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (t:Technology {id: $id})
            RETURN t.id AS id, t.name AS name, t.category AS category
            """,
            new { id });

        var row = await TrySingleAsync(cursor, ct);
        if (row is null)
        {
            return null;
        }

        var projectCount = await CountAsync(session, $"MATCH (t:Technology {{id: $id}})<-[:USES]-(:Project) RETURN count(*) AS c", new { id }, ct);
        var developerCount = await CountAsync(session, $"MATCH (t:Technology {{id: $id}})<-[:HAS_SKILL]-(:Developer) RETURN count(*) AS c", new { id }, ct);

        return new TechnologyDetailDto(
            id,
            row["name"].As<string>(),
            row["category"].As<string>(),
            projectCount,
            developerCount);
    }

    public async Task<IReadOnlyList<TechnologyDeveloperDto>> GetTechnologyDevelopersAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (t:Technology {id: $id})<-[h:HAS_SKILL]-(d:Developer)
            RETURN d.id AS developerId, d.name AS name, d.role AS role,
                   h.proficiency AS proficiency, h.since AS since
            ORDER BY d.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["developerId"].As<string>(), r["name"].As<string>(), r["role"].As<string>(), r["proficiency"].As<string?>(), r["since"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new TechnologyDeveloperDto(
                r.Item1,
                r.Item2,
                r.Item3,
                string.IsNullOrEmpty(r.Item4) ? "Intermediate" : r.Item4,
                r.Item5 ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<TechnologyProjectDto>> GetTechnologyProjectsAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (t:Technology {id: $id})<-[u:USES]-(p:Project)
            RETURN p.id AS projectId, p.name AS projectName, p.status AS status, u.purpose AS purpose
            ORDER BY p.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["projectId"].As<string>(), r["projectName"].As<string>(), r["status"].As<string>(), r["purpose"].As<string?>()), cancellationToken: ct);
        return rows
            .Select(r => new TechnologyProjectDto(r.Item1, r.Item2, r.Item3, string.IsNullOrEmpty(r.Item4) ? null : r.Item4))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetTechnologyCategoriesAsync(CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync("MATCH (t:Technology) RETURN DISTINCT t.category AS category ORDER BY category");
        return await cursor.ToListAsync(r => r["category"].As<string>(), cancellationToken: ct);
    }

    // ---- Organizations ----------------------------------------------------

    public async Task<IReadOnlyList<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var orgs = await session.RunAsync("MATCH (o:Organization) RETURN o.id AS id, o.name AS name ORDER BY o.name");

        var devCounts = await session.RunAsync(
            "MATCH (:Organization)<-[:WORKS_FOR]-(:Developer) RETURN o.id AS id, count(*) AS c",
            parameters: new Dictionary<string, object> { });

        var projCounts = await session.RunAsync(
            "MATCH (o:Organization)-[:OWNS]->(:Project) RETURN o.id AS id, count(*) AS c");

        Dictionary<string, int> devLookup = await ToCountLookupAsync(devCounts, ct);
        Dictionary<string, int> projLookup = await ToCountLookupAsync(projCounts, ct);

        var orgRows = await orgs.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>()), cancellationToken: ct);

        return orgRows
            .Select(r => new OrganizationSummaryDto(
                r.Item1,
                r.Item2,
                devLookup.GetValueOrDefault(r.Item1),
                projLookup.GetValueOrDefault(r.Item1)))
            .ToList();
    }

    public async Task<IReadOnlyList<DeveloperSummaryDto>> GetOrganizationDevelopersAsync(string id, CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (o:Organization {id: $id})<-[:WORKS_FOR]-(d:Developer)
            RETURN d.id AS id, d.name AS name, d.email AS email, d.role AS role
            ORDER BY d.name
            """,
            new { id });

        var rows = await cursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["email"].As<string>(), r["role"].As<string>()), cancellationToken: ct);
        return rows
            .Select(r => new DeveloperSummaryDto(r.Item1, r.Item2, r.Item3, r.Item4, null))
            .ToList();
    }

    // ---- Dashboard --------------------------------------------------------

    public async Task<DashboardCounts> GetDashboardCountsAsync(CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var developers = await CountLabelAsync(session, "Developer", ct);
        var projects = await CountLabelAsync(session, "Project", ct);
        var technologies = await CountLabelAsync(session, "Technology", ct);
        var repositories = await CountLabelAsync(session, "Repository", ct);
        var tasks = await CountLabelAsync(session, "Task", ct);
        var organizations = await CountLabelAsync(session, "Organization", ct);

        var relCursor = await session.RunAsync("MATCH ()-[r]->() RETURN type(r) AS type, count(*) AS c");
        var relationshipLabelCounts = (await relCursor.ToListAsync(ct))
            .ToDictionary(r => r["type"].As<string>(), r => r["c"].As<long>(), StringComparer.Ordinal);

        var worksFor = GetOrDefault(relationshipLabelCounts, "WORKS_FOR");
        var owns = GetOrDefault(relationshipLabelCounts, "OWNS");
        var worksOn = GetOrDefault(relationshipLabelCounts, "WORKS_ON");
        var uses = GetOrDefault(relationshipLabelCounts, "USES");
        var dependsOn = GetOrDefault(relationshipLabelCounts, "DEPENDS_ON");
        var hasSkill = GetOrDefault(relationshipLabelCounts, "HAS_SKILL");
        var contributedTo = GetOrDefault(relationshipLabelCounts, "CONTRIBUTED_TO");
        var requiresSkill = GetOrDefault(relationshipLabelCounts, "REQUIRES_SKILL");

        var relationships = (int)(worksFor + owns + worksOn + uses + dependsOn + hasSkill
            + contributedTo + requiresSkill + repositories + tasks);
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
        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync("MATCH (p:Project) RETURN p.status AS status, count(*) AS c");
        var rows = await cursor.ToListAsync(r => (r["status"].As<string>(), r["c"].As<long>()), cancellationToken: ct);
        return rows
            .Select(r => new ProjectStatusCountDto(r.Item1, (int)r.Item2))
            .OrderBy(x => x.Status)
            .ToList();
    }

    public async Task<IReadOnlyList<RelationshipTypeCountDto>> GetRelationshipTypeCountsAsync(CancellationToken ct)
    {
        await using var session = _driver.AsyncSession();

        var worksFor = await CountRelAsync(session, "WORKS_FOR", ct);
        var owns = await CountRelAsync(session, "OWNS", ct);
        var worksOn = await CountRelAsync(session, "WORKS_ON", ct);
        var uses = await CountRelAsync(session, "USES", ct);
        var dependsOn = await CountRelAsync(session, "DEPENDS_ON", ct);
        var hasSkill = await CountRelAsync(session, "HAS_SKILL", ct);
        var contributedTo = await CountRelAsync(session, "CONTRIBUTED_TO", ct);
        var requiresSkill = await CountRelAsync(session, "REQUIRES_SKILL", ct);
        var belongsTo = await CountLabelAsync(session, "Repository", ct);
        var hasTask = await CountLabelAsync(session, "Task", ct);

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
        await using var session = _driver.AsyncSession();

        var usageCursor = await session.RunAsync(
            "MATCH (t:Technology)<-[:USES]-(p:Project) RETURN t.id AS id, t.name AS name, t.category AS category, count(DISTINCT p) AS usage");
        var usageRows = await usageCursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["category"].As<string>(), r["usage"].As<long>()), cancellationToken: ct);

        var skillCursor = await session.RunAsync(
            "MATCH (t:Technology)<-[:HAS_SKILL]-(d:Developer) RETURN t.id AS id, count(DISTINCT d) AS skillCount");
        var skillLookup = (await skillCursor.ToListAsync(r => (r["id"].As<string>(), r["skillCount"].As<long>()), cancellationToken: ct))
            .ToDictionary(x => x.Item1, x => x.Item2);

        return usageRows
            .Select(r =>
            {
                var skillCount = skillLookup.GetValueOrDefault(r.Item1);
                return new CentralTechnologyDto(r.Item1, r.Item2, r.Item3, (int)r.Item4, (int)skillCount, (int)r.Item4 + (int)skillCount);
            })
            .OrderByDescending(x => x.Centrality)
            .ThenByDescending(x => x.ProjectUsage)
            .ThenBy(x => x.Name)
            .Take(limit)
            .ToList();
    }

    // ---- Search -----------------------------------------------------------

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<SearchResultDto>();
        }

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (n)
            WHERE (n:Developer AND n.name CONTAINS $query)
               OR (n:Project AND n.name CONTAINS $query)
               OR (n:Technology AND n.name CONTAINS $query)
               OR (n:Repository AND n.name CONTAINS $query)
            RETURN n.id AS id, n.name AS name,
                   CASE
                     WHEN n:Developer THEN 'Developer'
                     WHEN n:Project THEN 'Project'
                     WHEN n:Technology THEN 'Technology'
                     WHEN n:Repository THEN 'Repository'
                   END AS type,
                   CASE
                     WHEN n:Developer THEN n.role
                     WHEN n:Project THEN n.status
                     WHEN n:Technology THEN n.category
                     WHEN n:Repository THEN n.url
                   END AS subtitle
            ORDER BY n.name
            LIMIT $limit
            """,
            new { query, limit = (long)limit * 4 });

        var rows = await cursor.ToListAsync(r => (r["id"].As<string>(), r["name"].As<string>(), r["type"].As<string>(), r["subtitle"].As<string?>()), cancellationToken: ct);

        return rows
            .Select(r => new SearchResultDto(r.Item1, r.Item2, r.Item3, r.Item4 ?? string.Empty))
            .OrderBy(x => x.Name)
            .Take(limit)
            .ToList();
    }

    // ---- Graph explorer ---------------------------------------------------

    public async Task<GraphResponseDto?> GetGraphAsync(string id, int maxDepth, CancellationToken ct)
    {
        var (nodes, edges) = await BuildGraphAsync(ct);

        var startEntry = nodes.FirstOrDefault(kvp => kvp.Value.Id == id);
        if (startEntry.Key is null)
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
        var cursorKey = goalKey;
        while (cursorKey != startKey)
        {
            keys.Add(cursorKey);
            var (previous, edgeType) = visited[cursorKey];
            edgeTypes.Add(edgeType);
            cursorKey = previous;
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
        var pattern = label switch
        {
            "Developer" => "(n:Developer {id: $id})",
            "Project" => "(n:Project {id: $id})",
            "Technology" => "(n:Technology {id: $id})",
            "Organization" => "(n:Organization {id: $id})",
            "Repository" => "(n:Repository {id: $id})",
            "Task" => "(n:Task {id: $id})",
            _ => null,
        };

        if (pattern is null)
        {
            return false;
        }

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync($"MATCH {pattern} RETURN count(n) AS c", new { id });
        var count = await SingleLongAsync(cursor, "c", ct);
        return count > 0;
    }

    // ---- Internal helpers -------------------------------------------------

    private static long GetOrDefault(Dictionary<string, long> map, string key)
        => map.TryGetValue(key, out var value) ? value : 0;

    private static async Task<long> SingleLongAsync(IResultCursor cursor, string key, CancellationToken ct)
    {
        var list = await cursor.ToListAsync(cancellationToken: ct);
        if (list.Count == 0)
        {
            return 0;
        }

        return list[0][key].As<long>();
    }

    private static async Task<IRecord?> TrySingleAsync(IResultCursor cursor, CancellationToken ct)
    {
        var list = await cursor.ToListAsync(cancellationToken: ct);
        return list.Count == 0 ? null : list[0];
    }

    private static async Task<int> CountAsync(IAsyncSession session, string query, object parameters, CancellationToken ct)
    {
        var cursor = await session.RunAsync(query, parameters);
        var list = await cursor.ToListAsync(cancellationToken: ct);
        return list.Count == 0 ? 0 : (int)list[0]["c"].As<long>();
    }

    private static async Task<int> CountLabelAsync(IAsyncSession session, string label, CancellationToken ct)
    {
        var cursor = await session.RunAsync($"MATCH (n:{label}) RETURN count(n) AS c");
        var list = await cursor.ToListAsync(cancellationToken: ct);
        return list.Count == 0 ? 0 : (int)list[0]["c"].As<long>();
    }

    private static async Task<int> CountRelAsync(IAsyncSession session, string type, CancellationToken ct)
    {
        var cursor = await session.RunAsync($"MATCH ()-[r:{type}]->() RETURN count(r) AS c");
        var list = await cursor.ToListAsync(cancellationToken: ct);
        return list.Count == 0 ? 0 : (int)list[0]["c"].As<long>();
    }

    private static async Task<Dictionary<string, int>> ToCountLookupAsync(IResultCursor cursor, CancellationToken ct)
    {
        var list = await cursor.ToListAsync(cancellationToken: ct);
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var record in list)
        {
            result[record["id"].As<string>()] = (int)record["c"].As<long>();
        }

        return result;
    }

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

    private static string KeyOf(string type, string id) => type + "\u0001" + id;

    private async Task<(Dictionary<string, GraphNodeInfo> Nodes, List<GraphEdgeInfo> Edges)> BuildGraphAsync(CancellationToken ct)
    {
        var nodes = new Dictionary<string, GraphNodeInfo>(StringComparer.Ordinal);
        var edges = new List<GraphEdgeInfo>();

        await using var session = _driver.AsyncSession();

        var nodeCursor = await session.RunAsync(
            """
            MATCH (n)
            WHERE n:Organization OR n:Developer OR n:Project OR n:Technology OR n:Repository OR n:Task
            RETURN labels(n) AS labels, n.id AS id, n
            """);

        var nodeRows = await nodeCursor.ToListAsync(cancellationToken: ct);
        foreach (var record in nodeRows)
        {
            var labels = record["labels"].As<List<string>>();
            var id = record["id"].As<string>();
            var node = record["n"].As<INode>();

            var type = InferType(labels);

            var props = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (propName, value) in node.Properties)
            {
                props[propName] = value;
            }

            string name = type switch
            {
                "Task" => (node.Properties.TryGetValue("title", out var t) ? t as string : null) ?? id,
                _ => (node.Properties.TryGetValue("name", out var n) ? n as string : null) ?? id,
            };

            nodes[KeyOf(type, id)] = new GraphNodeInfo(type, id, name, KeyOf(type, id), props);
        }

        var relCursor = await session.RunAsync(
            """
            MATCH (a)-[r]->(b)
            WHERE a:Organization OR a:Developer OR a:Project OR a:Technology OR a:Repository OR a:Task
            RETURN type(r) AS type,
                   a.id AS sourceId, labels(a) AS sourceLabels,
                   b.id AS targetId, labels(b) AS targetLabels,
                   r
            """);

        var relRows = await relCursor.ToListAsync(cancellationToken: ct);
        foreach (var record in relRows)
        {
            var type = record["type"].As<string>();
            var sourceId = record["sourceId"].As<string>();
            var targetId = record["targetId"].As<string>();
            var sourceType = InferType(record["sourceLabels"].As<List<string>>());
            var targetType = InferType(record["targetLabels"].As<List<string>>());
            var rel = record["r"].As<IRelationship>();

            var props = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (propName, value) in rel.Properties)
            {
                props[propName] = value;
            }

            edges.Add(new GraphEdgeInfo(
                KeyOf(sourceType, sourceId),
                KeyOf(targetType, targetId),
                sourceId,
                targetId,
                type,
                props));
        }

        return (nodes, edges);
    }

    private static string InferType(List<string> labels)
    {
        if (labels.Contains("Organization")) { return "Organization"; }
        if (labels.Contains("Developer")) { return "Developer"; }
        if (labels.Contains("Project")) { return "Project"; }
        if (labels.Contains("Technology")) { return "Technology"; }
        if (labels.Contains("Repository")) { return "Repository"; }
        return "Task";
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
}
