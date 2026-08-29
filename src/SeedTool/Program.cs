using Neo4j.Driver;

var (uri, password) = ResolveCredentials(args);

Console.WriteLine($"Connecting to CognoDB ({uri}) ...");

using var driver = GraphDatabase.Driver(uri, AuthTokens.Basic("cognodb", password));

try
{
    await using var session = driver.AsyncSession();
    var probe = await session.RunAsync("RETURN 1");
    await probe.ConsumeAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unable to connect to the database: {ex.Message}");
    return 1;
}

Console.WriteLine("Connected to CognoDB.\n");

await EnsureConstraints(driver);
Console.WriteLine("Constraints ready.\n");

await Seed(driver);

Console.WriteLine("Seed completed successfully.");
return 0;

static (string Uri, string Password) ResolveCredentials(string[] args)
{
    var uri = args.Length > 0
        ? args[0]
        : Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
          ?? "bolt+s://db-5e76cc8b.bravo.databases.cognodb.com";

    var password = args.Length > 1
        ? args[1]
        : Environment.GetEnvironmentVariable("COGNODB_PASSWORD")
          ?? throw new InvalidOperationException(
              "Connection failed: no CognoDB password supplied. Set COGNODB_PASSWORD or pass it as the second argument.");

    return (uri, password);
}

static async Task EnsureConstraints(IDriver driver)
{
    const string constraints = """
        CREATE CONSTRAINT developer_id IF NOT EXISTS FOR (d:Developer) REQUIRE d.id IS UNIQUE;
        CREATE CONSTRAINT project_id IF NOT EXISTS FOR (p:Project) REQUIRE p.id IS UNIQUE;
        CREATE CONSTRAINT technology_id IF NOT EXISTS FOR (t:Technology) REQUIRE t.id IS UNIQUE;
        CREATE CONSTRAINT organization_id IF NOT EXISTS FOR (o:Organization) REQUIRE o.id IS UNIQUE;
        CREATE CONSTRAINT repository_id IF NOT EXISTS FOR (r:Repository) REQUIRE r.id IS UNIQUE;
        CREATE CONSTRAINT task_id IF NOT EXISTS FOR (task:Task) REQUIRE task.id IS UNIQUE;
        """;

    await using var session = driver.AsyncSession();
    await session.ExecuteWriteAsync(tx => tx.RunAsync(constraints));
}

static async Task Seed(IDriver driver)
{
    await using var session = driver.AsyncSession();

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (o:Organization {id: row.id})
        SET o.name = row.name
        """,
        new { rows = SeedData.Organizations }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (d:Developer {id: row.id})
        SET d.name = row.name, d.email = row.email, d.role = row.role
        """,
        new { rows = SeedData.Developers }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (p:Project {id: row.id})
        SET p.name = row.name, p.description = row.description, p.status = row.status
        """,
        new { rows = SeedData.Projects }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (t:Technology {id: row.id})
        SET t.name = row.name, t.category = row.category
        """,
        new { rows = SeedData.Technologies }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (r:Repository {id: row.id})
        SET r.name = row.name, r.url = row.url
        WITH r, row
        MATCH (p:Project {id: row.projectId})
        MERGE (r)-[:BELONGS_TO]->(p)
        """,
        new { rows = SeedData.Repositories }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MERGE (t:Task {id: row.id})
        SET t.title = row.title, t.status = row.status, t.priority = row.priority
        WITH t, row
        MATCH (p:Project {id: row.projectId})
        MERGE (p)-[:HAS_TASK]->(t)
        """,
        new { rows = SeedData.Tasks }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (d:Developer {id: row.developerId})
        MATCH (o:Organization {id: row.organizationId})
        MERGE (d)-[:WORKS_FOR {since: row.since}]->(o)
        """,
        new { rows = SeedData.WorksFor }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (o:Organization {id: row.organizationId})
        MATCH (p:Project {id: row.projectId})
        MERGE (o)-[:OWNS]->(p)
        """,
        new { rows = SeedData.Owns }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (d:Developer {id: row.developerId})
        MATCH (p:Project {id: row.projectId})
        MERGE (d)-[r:WORKS_ON]->(p)
        SET r.role = row.role, r.since = row.since
        """,
        new { rows = SeedData.WorksOn }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (p:Project {id: row.projectId})
        MATCH (t:Technology {id: row.technologyId})
        MERGE (p)-[r:USES]->(t)
        SET r.purpose = row.purpose
        """,
        new { rows = SeedData.Uses }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (p:Project {id: row.projectId})
        MATCH (dep:Project {id: row.dependencyProjectId})
        MERGE (p)-[:DEPENDS_ON]->(dep)
        """,
        new { rows = SeedData.DependsOn }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (d:Developer {id: row.developerId})
        MATCH (t:Technology {id: row.technologyId})
        MERGE (d)-[r:HAS_SKILL]->(t)
        SET r.proficiency = row.proficiency, r.since = row.since
        """,
        new { rows = SeedData.HasSkill }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (d:Developer {id: row.developerId})
        MATCH (r:Repository {id: row.repositoryId})
        MERGE (d)-[c:CONTRIBUTED_TO]->(r)
        SET c.contributionCount = row.contributionCount, c.since = row.since
        """,
        new { rows = SeedData.ContributedTo }));

    await session.ExecuteWriteAsync(tx => tx.RunAsync(
        """
        UNWIND $rows AS row
        MATCH (t:Task {id: row.taskId})
        MATCH (tech:Technology {id: row.technologyId})
        MERGE (t)-[:REQUIRES_SKILL]->(tech)
        """,
        new { rows = SeedData.RequiresSkill }));
}

// ---------------------------------------------------------------------------
// Seed data
// ---------------------------------------------------------------------------

static class SeedData
{
public static readonly object[] Organizations =
{
    new { id = "o1", name = "Acme Corp" },
    new { id = "o2", name = "Nimbus Labs" },
    new { id = "o3", name = "Helix Industries" },
    new { id = "o4", name = "Beacon Soft" },
    new { id = "o5", name = "OpenVox" },
    new { id = "o6", name = "Cobalt Systems" },
    new { id = "o7", name = "Vertex Robotics" },
    new { id = "o8", name = "Zephyr Media" },
    new { id = "o9", name = "Ironclad Logistics" },
};

public static readonly object[] Developers =
{
    new { id = "d1", name = "Alice Chen", email = "alice.chen@example.com", role = "Backend Engineer" },
    new { id = "d2", name = "Marcus Johnson", email = "marcus.johnson@example.com", role = "Frontend Engineer" },
    new { id = "d3", name = "Priya Sharma", email = "priya.sharma@example.com", role = "Full-Stack Engineer" },
    new { id = "d4", name = "Diego Martinez", email = "diego.martinez@example.com", role = "DevOps Engineer" },
    new { id = "d5", name = "Emily Watson", email = "emily.watson@example.com", role = "Data Engineer" },
    new { id = "d6", name = "Omar Farouk", email = "omar.farouk@example.com", role = "ML Engineer" },
    new { id = "d7", name = "Sofia Rossi", email = "sofia.rossi@example.com", role = "QA Engineer" },
    new { id = "d8", name = "Liam O'Brien", email = "liam.obrien@example.com", role = "Backend Engineer" },
    new { id = "d9", name = "Yuki Tanaka", email = "yuki.tanaka@example.com", role = "Frontend Engineer" },
    new { id = "d10", name = "Aisha Bello", email = "aisha.bello@example.com", role = "Product Engineer" },
    new { id = "d11", name = "Noah Kim", email = "noah.kim@example.com", role = "DevOps Engineer" },
    new { id = "d12", name = "Freya Novak", email = "freya.novak@example.com", role = "Data Engineer" },
    new { id = "d13", name = "Kenji Sato", email = "kenji.sato@example.com", role = "Mobile Engineer" },
    new { id = "d14", name = "Lucia Blanco", email = "lucia.blanco@example.com", role = "Engineering Manager" },
    new { id = "d15", name = "Tariq Haddad", email = "tariq.haddad@example.com", role = "Security Engineer" },
    new { id = "d16", name = "Emma Larsson", email = "emma.larsson@example.com", role = "QA Engineer" },
    new { id = "d17", name = "Devin Carter", email = "devin.carter@example.com", role = "Backend Engineer" },
    new { id = "d18", name = "Nina Petrov", email = "nina.petrov@example.com", role = "Data Engineer" },
    new { id = "d19", name = "Ravi Menon", email = "ravi.menon@example.com", role = "Mobile Engineer" },
    new { id = "d20", name = "Hannah Weiss", email = "hannah.weiss@example.com", role = "Frontend Engineer" },
    new { id = "d21", name = "Tomás Silva", email = "tomas.silva@example.com", role = "ML Engineer" },
    new { id = "d22", name = "Zara Haddad", email = "zara.haddad@example.com", role = "DevOps Engineer" },
    new { id = "d23", name = "Elena Moreau", email = "elena.moreau@example.com", role = "QA Engineer" },
    new { id = "d24", name = "Samir Nasser", email = "samir.nasser@example.com", role = "Full-Stack Engineer" },
    new { id = "d25", name = "Ingrid Sørensen", email = "ingrid.sorensen@example.com", role = "Security Engineer" },
    new { id = "d26", name = "Petra Klein", email = "petra.klein@example.com", role = "Backend Engineer" },
    new { id = "d27", name = "Jonas Lindqvist", email = "jonas.lindqvist@example.com", role = "Data Engineer" },
    new { id = "d28", name = "Amara Okafor", email = "amara.okafor@example.com", role = "Frontend Engineer" },
};

public static readonly object[] Projects =
{
    new { id = "p1", name = "Atlas ERP", description = "Enterprise resource planning suite for mid-size organisations.", status = "active" },
    new { id = "p2", name = "Nova Analytics Platform", description = "Self-service BI and analytics over streaming and batch data.", status = "active" },
    new { id = "p3", name = "Pulse Health Monitor", description = "Real-time patient vitals monitoring with alerting.", status = "active" },
    new { id = "p4", name = "Quantum Commerce", description = "High-volume e-commerce storefront and checkout platform.", status = "in_progress" },
    new { id = "p5", name = "Sentinel Security Suite", description = "Vulnerability scanning and threat intelligence platform.", status = "active" },
    new { id = "p6", name = "Orbit Task Scheduler", description = "Distributed, dependency-aware job scheduler.", status = "in_progress" },
    new { id = "p7", name = "Flux Data Pipeline", description = "Replayable stream ingestion and enrichment pipeline.", status = "maintenance" },
    new { id = "p8", name = "Helios Chat Platform", description = "Real-time chat and presence platform with bot integrations.", status = "planning" },
    new { id = "p9", name = "Zenith Mobile Wallet", description = "Digital wallet with P2P transfers and fraud detection.", status = "in_progress" },
    new { id = "p10", name = "Aurora Data Lake", description = "Centralised data lake with governed access and lineage.", status = "active" },
    new { id = "p11", name = "Cobalt CI/CD", description = "Continuous integration and delivery pipelines as a service.", status = "active" },
    new { id = "p12", name = "Vega Fitness Tracker", description = "Wearable fitness tracking with social challenges.", status = "planning" },
    new { id = "p13", name = "Aegis Identity", description = "Central identity and access management platform.", status = "in_progress" },
    new { id = "p14", name = "Boreal EDI", description = "Electronic data interchange gateway for logistics.", status = "maintenance" },
    new { id = "p15", name = "Stratos Edge Mesh", description = "Edge computing mesh for low-latency IoT workloads.", status = "in_progress" },
};

public static readonly object[] Technologies =
{
    new { id = "t01", name = ".NET / C#", category = "Backend" },
    new { id = "t02", name = "Java", category = "Backend" },
    new { id = "t03", name = "Python", category = "Backend" },
    new { id = "t04", name = "Go", category = "Backend" },
    new { id = "t05", name = "TypeScript", category = "Frontend" },
    new { id = "t06", name = "React", category = "Frontend" },
    new { id = "t07", name = "Angular", category = "Frontend" },
    new { id = "t08", name = "Node.js", category = "Backend" },
    new { id = "t09", name = "PostgreSQL", category = "Database" },
    new { id = "t10", name = "MongoDB", category = "Database" },
    new { id = "t11", name = "Neo4j", category = "Database" },
    new { id = "t12", name = "Redis", category = "Infrastructure" },
    new { id = "t13", name = "Docker", category = "DevOps" },
    new { id = "t14", name = "Kubernetes", category = "DevOps" },
    new { id = "t15", name = "AWS", category = "Cloud" },
    new { id = "t16", name = "Azure", category = "Cloud" },
    new { id = "t17", name = "TensorFlow", category = "ML" },
    new { id = "t18", name = "GraphQL", category = "API" },
    new { id = "t19", name = "Apache Kafka", category = "Streaming" },
    new { id = "t20", name = "Apache Spark", category = "Big Data" },
    new { id = "t21", name = "RabbitMQ", category = "Infrastructure" },
    new { id = "t22", name = "Flink", category = "Streaming" },
    new { id = "t23", name = "PyTorch", category = "ML" },
    new { id = "t24", name = "gRPC", category = "API" },
    new { id = "t25", name = "Cassandra", category = "Database" },
    new { id = "t26", name = "Rust", category = "Backend" },
};

public static readonly object[] Repositories =
{
    new { id = "r01", name = "atlas-api", url = "https://github.com/acme/atlas-api", projectId = "p1" },
    new { id = "r02", name = "atlas-web", url = "https://github.com/acme/atlas-web", projectId = "p1" },
    new { id = "r03", name = "atlas-infra", url = "https://github.com/acme/atlas-infra", projectId = "p1" },
    new { id = "r04", name = "nova-query-engine", url = "https://github.com/acme/nova-query-engine", projectId = "p2" },
    new { id = "r05", name = "nova-dashboard", url = "https://github.com/acme/nova-dashboard", projectId = "p2" },
    new { id = "r06", name = "pulse-backend", url = "https://github.com/beacon/pulse-backend", projectId = "p3" },
    new { id = "r07", name = "pulse-mobile", url = "https://github.com/beacon/pulse-mobile", projectId = "p3" },
    new { id = "r08", name = "quantum-storefront", url = "https://github.com/nimbus/quantum-storefront", projectId = "p4" },
    new { id = "r09", name = "quantum-checkout", url = "https://github.com/nimbus/quantum-checkout", projectId = "p4" },
    new { id = "r10", name = "sentinel-scanner", url = "https://github.com/openvox/sentinel-scanner", projectId = "p5" },
    new { id = "r11", name = "orbit-scheduler", url = "https://github.com/helix/orbit-scheduler", projectId = "p6" },
    new { id = "r12", name = "flux-pipeline", url = "https://github.com/acme/flux-pipeline", projectId = "p7" },
    new { id = "r13", name = "helios-gateway", url = "https://github.com/beacon/helios-gateway", projectId = "p8" },
    new { id = "r14", name = "zenith-wallet-app", url = "https://github.com/nimbus/zenith-wallet-app", projectId = "p9" },
    new { id = "r15", name = "aurora-lake", url = "https://github.com/cobalt/aurora-lake", projectId = "p10" },
    new { id = "r16", name = "cobalt-cicd", url = "https://github.com/cobalt/cobalt-cicd", projectId = "p11" },
    new { id = "r17", name = "vega-tracker-app", url = "https://github.com/vertex/vega-tracker-app", projectId = "p12" },
    new { id = "r18", name = "aegis-idp", url = "https://github.com/openvox/aegis-idp", projectId = "p13" },
    new { id = "r19", name = "boreal-edi-gateway", url = "https://github.com/ironclad/boreal-edi-gateway", projectId = "p14" },
    new { id = "r20", name = "stratos-edge-mesh", url = "https://github.com/vertex/stratos-edge-mesh", projectId = "p15" },
    new { id = "r21", name = "zephyr-media-api", url = "https://github.com/zephyr/zephyr-media-api", projectId = "p12" },
    new { id = "r22", name = "cobalt-observability", url = "https://github.com/cobalt/cobalt-observability", projectId = "p11" },
};

public static readonly object[] Tasks =
{
    new { id = "sk1", title = "Build inventory ledger API", status = "done", priority = 1, projectId = "p1" },
    new { id = "sk2", title = "Implement OAuth2 SSO", status = "in_progress", priority = 2, projectId = "p1" },
    new { id = "sk3", title = "Migrate customer records", status = "todo", priority = 3, projectId = "p1" },
    new { id = "sk4", title = "Add reporting endpoints", status = "backlog", priority = 4, projectId = "p1" },
    new { id = "sk5", title = "Design query engine DSL", status = "done", priority = 1, projectId = "p2" },
    new { id = "sk6", title = "Build dashboard widgets", status = "in_progress", priority = 2, projectId = "p2" },
    new { id = "sk7", title = "Connect streaming ingestion", status = "todo", priority = 2, projectId = "p2" },
    new { id = "sk8", title = "Alerting rules engine", status = "backlog", priority = 3, projectId = "p2" },
    new { id = "sk9", title = "Implement vitals ingestion", status = "done", priority = 1, projectId = "p3" },
    new { id = "sk10", title = "Build alerting pipeline", status = "in_progress", priority = 2, projectId = "p3" },
    new { id = "sk11", title = "Mobile push notifications", status = "todo", priority = 3, projectId = "p3" },
    new { id = "sk12", title = "Cart checkout flow", status = "in_progress", priority = 1, projectId = "p4" },
    new { id = "sk13", title = "Payment gateway integration", status = "todo", priority = 1, projectId = "p4" },
    new { id = "sk14", title = "Product catalog search", status = "done", priority = 2, projectId = "p4" },
    new { id = "sk15", title = "Build vulnerability scanner", status = "done", priority = 1, projectId = "p5" },
    new { id = "sk16", title = "Connect threat intel feed", status = "in_progress", priority = 2, projectId = "p5" },
    new { id = "sk17", title = "Generate compliance reports", status = "todo", priority = 3, projectId = "p5" },
    new { id = "sk18", title = "Scheduler core loop", status = "in_progress", priority = 1, projectId = "p6" },
    new { id = "sk19", title = "Retry policy engine", status = "todo", priority = 2, projectId = "p6" },
    new { id = "sk20", title = "Service integration hooks", status = "backlog", priority = 3, projectId = "p6" },
    new { id = "sk21", title = "Audit log exporter", status = "backlog", priority = 4, projectId = "p6" },
    new { id = "sk22", title = "Replayable pipeline stages", status = "done", priority = 1, projectId = "p7" },
    new { id = "sk23", title = "Schema registry", status = "in_progress", priority = 2, projectId = "p7" },
    new { id = "sk24", title = "Dead letter queue", status = "todo", priority = 3, projectId = "p7" },
    new { id = "sk25", title = "Chat message routing", status = "todo", priority = 1, projectId = "p8" },
    new { id = "sk26", title = "Real-time presence", status = "backlog", priority = 2, projectId = "p8" },
    new { id = "sk27", title = "Bot framework integration", status = "backlog", priority = 3, projectId = "p8" },
    new { id = "sk28", title = "Wallet balance cache", status = "in_progress", priority = 1, projectId = "p9" },
    new { id = "sk29", title = "P2P transfers", status = "todo", priority = 1, projectId = "p9" },
    new { id = "sk30", title = "KYC verification", status = "todo", priority = 2, projectId = "p9" },
    new { id = "sk31", title = "Fraud detection rules", status = "backlog", priority = 3, projectId = "p9" },
    new { id = "sk32", title = "Lake ingestion pipelines", status = "in_progress", priority = 1, projectId = "p10" },
    new { id = "sk33", title = "Columnar tables & partitioning", status = "todo", priority = 2, projectId = "p10" },
    new { id = "sk34", title = "Data lineage catalog", status = "backlog", priority = 3, projectId = "p10" },
    new { id = "sk35", title = "Pipeline orchestrator service", status = "in_progress", priority = 1, projectId = "p11" },
    new { id = "sk36", title = "Artifact registry", status = "todo", priority = 2, projectId = "p11" },
    new { id = "sk37", title = "Observability dashboards", status = "done", priority = 3, projectId = "p11" },
    new { id = "sk38", title = "Sensor data ingestion", status = "todo", priority = 1, projectId = "p12" },
    new { id = "sk39", title = "Activity rings UI", status = "backlog", priority = 2, projectId = "p12" },
    new { id = "sk40", title = "SSO integration", status = "in_progress", priority = 1, projectId = "p13" },
    new { id = "sk41", title = "MFA enforcement", status = "todo", priority = 1, projectId = "p13" },
    new { id = "sk42", title = "Role-based access controls", status = "done", priority = 2, projectId = "p13" },
    new { id = "sk43", title = "EDI message transformation", status = "done", priority = 1, projectId = "p14" },
    new { id = "sk44", title = "Partner onboarding API", status = "in_progress", priority = 2, projectId = "p14" },
    new { id = "sk45", title = "Edge node deployment", status = "todo", priority = 1, projectId = "p15" },
    new { id = "sk46", title = "Low-latency sync protocol", status = "backlog", priority = 2, projectId = "p15" },
};

public static readonly object[] WorksFor =
{
    new { developerId = "d1", organizationId = "o1", since = "2020" }, new { developerId = "d2", organizationId = "o1", since = "2020" },
    new { developerId = "d5", organizationId = "o1", since = "2020" }, new { developerId = "d9", organizationId = "o1", since = "2020" },
    new { developerId = "d14", organizationId = "o1", since = "2020" }, new { developerId = "d3", organizationId = "o2", since = "2020" },
    new { developerId = "d10", organizationId = "o2", since = "2020" }, new { developerId = "d13", organizationId = "o2", since = "2020" },
    new { developerId = "d4", organizationId = "o3", since = "2020" }, new { developerId = "d8", organizationId = "o3", since = "2020" },
    new { developerId = "d11", organizationId = "o3", since = "2020" }, new { developerId = "d6", organizationId = "o4", since = "2020" },
    new { developerId = "d12", organizationId = "o4", since = "2020" }, new { developerId = "d16", organizationId = "o4", since = "2020" },
    new { developerId = "d7", organizationId = "o5", since = "2020" }, new { developerId = "d15", organizationId = "o5", since = "2020" },
    new { developerId = "d17", organizationId = "o6", since = "2020" }, new { developerId = "d20", organizationId = "o6", since = "2020" },
    new { developerId = "d24", organizationId = "o6", since = "2020" }, new { developerId = "d28", organizationId = "o6", since = "2020" },
    new { developerId = "d18", organizationId = "o7", since = "2020" }, new { developerId = "d19", organizationId = "o7", since = "2020" },
    new { developerId = "d21", organizationId = "o7", since = "2020" }, new { developerId = "d22", organizationId = "o8", since = "2020" },
    new { developerId = "d23", organizationId = "o8", since = "2020" }, new { developerId = "d25", organizationId = "o9", since = "2020" },
    new { developerId = "d26", organizationId = "o9", since = "2020" }, new { developerId = "d27", organizationId = "o9", since = "2020" },
};

public static readonly object[] Owns =
{
    new { organizationId = "o1", projectId = "p1" }, new { organizationId = "o1", projectId = "p2" }, new { organizationId = "o1", projectId = "p7" },
    new { organizationId = "o2", projectId = "p4" }, new { organizationId = "o2", projectId = "p9" },
    new { organizationId = "o3", projectId = "p6" },
    new { organizationId = "o4", projectId = "p3" }, new { organizationId = "o4", projectId = "p8" },
    new { organizationId = "o5", projectId = "p5" }, new { organizationId = "o5", projectId = "p13" },
    new { organizationId = "o6", projectId = "p10" }, new { organizationId = "o6", projectId = "p11" },
    new { organizationId = "o7", projectId = "p12" }, new { organizationId = "o7", projectId = "p15" },
    new { organizationId = "o8", projectId = "p12" },
    new { organizationId = "o9", projectId = "p14" },
};

public static readonly object[] WorksOn =
{
    new { developerId = "d1", projectId = "p1", role = "Lead", since = "2021" }, new { developerId = "d1", projectId = "p6", role = "Contributor", since = "2023" },
    new { developerId = "d2", projectId = "p2", role = "Lead", since = "2022" }, new { developerId = "d2", projectId = "p8", role = "Contributor", since = "2024" },
    new { developerId = "d3", projectId = "p4", role = "Lead", since = "2023" }, new { developerId = "d3", projectId = "p1", role = "Contributor", since = "2021" },
    new { developerId = "d4", projectId = "p1", role = "Infra", since = "2021" }, new { developerId = "d4", projectId = "p5", role = "Member", since = "2022" },
    new { developerId = "d5", projectId = "p2", role = "Lead", since = "2022" }, new { developerId = "d5", projectId = "p7", role = "Contributor", since = "2020" },
    new { developerId = "d6", projectId = "p3", role = "Lead", since = "2022" }, new { developerId = "d6", projectId = "p8", role = "Contributor", since = "2024" },
    new { developerId = "d7", projectId = "p1", role = "QA", since = "2021" }, new { developerId = "d7", projectId = "p4", role = "QA", since = "2023" },
    new { developerId = "d8", projectId = "p6", role = "Lead", since = "2023" }, new { developerId = "d8", projectId = "p5", role = "Contributor", since = "2022" },
    new { developerId = "d9", projectId = "p8", role = "Lead", since = "2024" }, new { developerId = "d9", projectId = "p2", role = "Contributor", since = "2022" },
    new { developerId = "d10", projectId = "p4", role = "Product", since = "2023" }, new { developerId = "d10", projectId = "p9", role = "Lead", since = "2024" },
    new { developerId = "d11", projectId = "p5", role = "DevOps", since = "2022" }, new { developerId = "d11", projectId = "p2", role = "Contributor", since = "2023" },
    new { developerId = "d12", projectId = "p7", role = "Lead", since = "2020" }, new { developerId = "d12", projectId = "p9", role = "Contributor", since = "2024" },
    new { developerId = "d13", projectId = "p9", role = "Mobile", since = "2024" }, new { developerId = "d13", projectId = "p3", role = "Contributor", since = "2022" },
    new { developerId = "d14", projectId = "p1", role = "Manager", since = "2021" }, new { developerId = "d14", projectId = "p3", role = "Manager", since = "2022" },
    new { developerId = "d15", projectId = "p5", role = "Security", since = "2022" }, new { developerId = "d15", projectId = "p1", role = "Consultant", since = "2021" },
    new { developerId = "d16", projectId = "p5", role = "QA", since = "2022" }, new { developerId = "d16", projectId = "p9", role = "QA", since = "2024" },
    new { developerId = "d17", projectId = "p1", role = "Contributor", since = "2022" }, new { developerId = "d17", projectId = "p11", role = "Lead", since = "2023" },
    new { developerId = "d18", projectId = "p10", role = "Lead", since = "2023" }, new { developerId = "d18", projectId = "p7", role = "Contributor", since = "2020" },
    new { developerId = "d19", projectId = "p12", role = "Mobile", since = "2024" }, new { developerId = "d19", projectId = "p9", role = "Contributor", since = "2024" },
    new { developerId = "d20", projectId = "p8", role = "Contributor", since = "2024" }, new { developerId = "d20", projectId = "p12", role = "Frontend", since = "2024" },
    new { developerId = "d21", projectId = "p2", role = "ML", since = "2023" }, new { developerId = "d21", projectId = "p10", role = "Contributor", since = "2023" },
    new { developerId = "d22", projectId = "p13", role = "DevOps", since = "2023" }, new { developerId = "d22", projectId = "p11", role = "Contributor", since = "2023" },
    new { developerId = "d23", projectId = "p13", role = "QA", since = "2023" }, new { developerId = "d23", projectId = "p15", role = "QA", since = "2024" },
    new { developerId = "d24", projectId = "p14", role = "Lead", since = "2022" }, new { developerId = "d24", projectId = "p1", role = "Contributor", since = "2022" },
    new { developerId = "d25", projectId = "p13", role = "Security", since = "2023" }, new { developerId = "d25", projectId = "p5", role = "Consultant", since = "2023" },
    new { developerId = "d26", projectId = "p15", role = "Lead", since = "2024" }, new { developerId = "d26", projectId = "p13", role = "Contributor", since = "2023" },
    new { developerId = "d27", projectId = "p10", role = "Data", since = "2023" }, new { developerId = "d27", projectId = "p14", role = "Contributor", since = "2024" },
    new { developerId = "d28", projectId = "p11", role = "Frontend", since = "2023" }, new { developerId = "d28", projectId = "p15", role = "Contributor", since = "2024" },
};

public static readonly object[] Uses =
{
    new { projectId = "p1", technologyId = "t01", purpose = "build" }, new { projectId = "p1", technologyId = "t05", purpose = "frontend" }, new { projectId = "p1", technologyId = "t09", purpose = "data" }, new { projectId = "p1", technologyId = "t13", purpose = "deployment" },
    new { projectId = "p2", technologyId = "t03", purpose = "processing" }, new { projectId = "p2", technologyId = "t19", purpose = "streaming" }, new { projectId = "p2", technologyId = "t20", purpose = "batch" }, new { projectId = "p2", technologyId = "t05", purpose = "frontend" },
    new { projectId = "p3", technologyId = "t08", purpose = "runtime" }, new { projectId = "p3", technologyId = "t10", purpose = "storage" }, new { projectId = "p3", technologyId = "t12", purpose = "caching" }, new { projectId = "p3", technologyId = "t17", purpose = "prediction" },
    new { projectId = "p4", technologyId = "t01", purpose = "build" }, new { projectId = "p4", technologyId = "t06", purpose = "frontend" }, new { projectId = "p4", technologyId = "t09", purpose = "data" }, new { projectId = "p4", technologyId = "t12", purpose = "caching" },
    new { projectId = "p5", technologyId = "t02", purpose = "build" }, new { projectId = "p5", technologyId = "t04", purpose = "scanner" }, new { projectId = "p5", technologyId = "t16", purpose = "cloud" },
    new { projectId = "p6", technologyId = "t08", purpose = "runtime" }, new { projectId = "p6", technologyId = "t11", purpose = "dependencies" }, new { projectId = "p6", technologyId = "t12", purpose = "queueing" },
    new { projectId = "p7", technologyId = "t03", purpose = "processing" }, new { projectId = "p7", technologyId = "t19", purpose = "streaming" }, new { projectId = "p7", technologyId = "t20", purpose = "batch" }, new { projectId = "p7", technologyId = "t15", purpose = "cloud" },
    new { projectId = "p8", technologyId = "t05", purpose = "build" }, new { projectId = "p8", technologyId = "t08", purpose = "runtime" }, new { projectId = "p8", technologyId = "t11", purpose = "presence" }, new { projectId = "p8", technologyId = "t18", purpose = "api" },
    new { projectId = "p9", technologyId = "t04", purpose = "api" }, new { projectId = "p9", technologyId = "t03", purpose = "services" }, new { projectId = "p9", technologyId = "t14", purpose = "infrastructure" }, new { projectId = "p9", technologyId = "t15", purpose = "cloud" },
    new { projectId = "p1", technologyId = "t24", purpose = "api" }, new { projectId = "p3", technologyId = "t21", purpose = "messaging" }, new { projectId = "p4", technologyId = "t15", purpose = "cloud" }, new { projectId = "p5", technologyId = "t25", purpose = "data" },
    new { projectId = "p6", technologyId = "t21", purpose = "queueing" }, new { projectId = "p8", technologyId = "t24", purpose = "api" },
    new { projectId = "p10", technologyId = "t20", purpose = "batch" }, new { projectId = "p10", technologyId = "t22", purpose = "streaming" }, new { projectId = "p10", technologyId = "t25", purpose = "storage" }, new { projectId = "p10", technologyId = "t15", purpose = "cloud" },
    new { projectId = "p11", technologyId = "t13", purpose = "containers" }, new { projectId = "p11", technologyId = "t14", purpose = "orchestration" }, new { projectId = "p11", technologyId = "t01", purpose = "build" }, new { projectId = "p11", technologyId = "t05", purpose = "frontend" },
    new { projectId = "p12", technologyId = "t08", purpose = "runtime" }, new { projectId = "p12", technologyId = "t06", purpose = "frontend" }, new { projectId = "p12", technologyId = "t10", purpose = "storage" }, new { projectId = "p12", technologyId = "t23", purpose = "prediction" },
    new { projectId = "p13", technologyId = "t05", purpose = "frontend" }, new { projectId = "p13", technologyId = "t01", purpose = "services" }, new { projectId = "p13", technologyId = "t12", purpose = "sessions" }, new { projectId = "p13", technologyId = "t18", purpose = "api" },
    new { projectId = "p14", technologyId = "t02", purpose = "gateway" }, new { projectId = "p14", technologyId = "t21", purpose = "messaging" }, new { projectId = "p14", technologyId = "t09", purpose = "storage" }, new { projectId = "p14", technologyId = "t15", purpose = "cloud" },
    new { projectId = "p15", technologyId = "t26", purpose = "runtime" }, new { projectId = "p15", technologyId = "t24", purpose = "mesh" }, new { projectId = "p15", technologyId = "t13", purpose = "deployment" }, new { projectId = "p15", technologyId = "t16", purpose = "cloud" },
};

public static readonly object[] DependsOn =
{
    new { projectId = "p2", dependencyProjectId = "p7" }, new { projectId = "p2", dependencyProjectId = "p1" },
    new { projectId = "p3", dependencyProjectId = "p1" },
    new { projectId = "p4", dependencyProjectId = "p1" },
    new { projectId = "p5", dependencyProjectId = "p1" },
    new { projectId = "p6", dependencyProjectId = "p1" },
    new { projectId = "p7", dependencyProjectId = "p1" },
    new { projectId = "p8", dependencyProjectId = "p6" },
    new { projectId = "p9", dependencyProjectId = "p4" }, new { projectId = "p9", dependencyProjectId = "p8" },
    new { projectId = "p10", dependencyProjectId = "p7" }, new { projectId = "p10", dependencyProjectId = "p2" },
    new { projectId = "p11", dependencyProjectId = "p6" }, new { projectId = "p11", dependencyProjectId = "p1" },
    new { projectId = "p12", dependencyProjectId = "p8" },
    new { projectId = "p13", dependencyProjectId = "p1" }, new { projectId = "p13", dependencyProjectId = "p5" },
    new { projectId = "p14", dependencyProjectId = "p7" },
    new { projectId = "p15", dependencyProjectId = "p13" }, new { projectId = "p15", dependencyProjectId = "p11" },
};

public static readonly object[] HasSkill =
{
    new { developerId = "d1", technologyId = "t01", proficiency = "Expert", since = "2018" }, new { developerId = "d1", technologyId = "t09", proficiency = "Advanced", since = "2019" }, new { developerId = "d1", technologyId = "t05", proficiency = "Proficient", since = "2020" }, new { developerId = "d1", technologyId = "t11", proficiency = "Advanced", since = "2021" },
    new { developerId = "d2", technologyId = "t05", proficiency = "Expert", since = "2019" }, new { developerId = "d2", technologyId = "t06", proficiency = "Advanced", since = "2020" }, new { developerId = "d2", technologyId = "t07", proficiency = "Advanced", since = "2019" }, new { developerId = "d2", technologyId = "t18", proficiency = "Proficient", since = "2021" },
    new { developerId = "d3", technologyId = "t03", proficiency = "Advanced", since = "2018" }, new { developerId = "d3", technologyId = "t05", proficiency = "Advanced", since = "2019" }, new { developerId = "d3", technologyId = "t01", proficiency = "Proficient", since = "2020" }, new { developerId = "d3", technologyId = "t09", proficiency = "Proficient", since = "2020" },
    new { developerId = "d4", technologyId = "t13", proficiency = "Advanced", since = "2019" }, new { developerId = "d4", technologyId = "t14", proficiency = "Expert", since = "2020" }, new { developerId = "d4", technologyId = "t15", proficiency = "Proficient", since = "2021" },
    new { developerId = "d5", technologyId = "t03", proficiency = "Expert", since = "2018" }, new { developerId = "d5", technologyId = "t20", proficiency = "Advanced", since = "2019" }, new { developerId = "d5", technologyId = "t19", proficiency = "Proficient", since = "2021" }, new { developerId = "d5", technologyId = "t09", proficiency = "Advanced", since = "2018" },
    new { developerId = "d6", technologyId = "t03", proficiency = "Advanced", since = "2018" }, new { developerId = "d6", technologyId = "t17", proficiency = "Expert", since = "2019" }, new { developerId = "d6", technologyId = "t20", proficiency = "Proficient", since = "2020" },
    new { developerId = "d7", technologyId = "t05", proficiency = "Proficient", since = "2020" }, new { developerId = "d7", technologyId = "t06", proficiency = "Advanced", since = "2021" },
    new { developerId = "d8", technologyId = "t01", proficiency = "Advanced", since = "2018" }, new { developerId = "d8", technologyId = "t08", proficiency = "Proficient", since = "2020" }, new { developerId = "d8", technologyId = "t09", proficiency = "Advanced", since = "2019" }, new { developerId = "d8", technologyId = "t11", proficiency = "Proficient", since = "2021" },
    new { developerId = "d9", technologyId = "t05", proficiency = "Expert", since = "2018" }, new { developerId = "d9", technologyId = "t07", proficiency = "Expert", since = "2019" }, new { developerId = "d9", technologyId = "t06", proficiency = "Advanced", since = "2020" }, new { developerId = "d9", technologyId = "t18", proficiency = "Advanced", since = "2021" },
    new { developerId = "d10", technologyId = "t03", proficiency = "Proficient", since = "2020" }, new { developerId = "d10", technologyId = "t05", proficiency = "Expert", since = "2018" }, new { developerId = "d10", technologyId = "t11", proficiency = "Proficient", since = "2021" }, new { developerId = "d10", technologyId = "t18", proficiency = "Advanced", since = "2019" },
    new { developerId = "d11", technologyId = "t14", proficiency = "Advanced", since = "2020" }, new { developerId = "d11", technologyId = "t13", proficiency = "Advanced", since = "2021" }, new { developerId = "d11", technologyId = "t15", proficiency = "Expert", since = "2019" }, new { developerId = "d11", technologyId = "t04", proficiency = "Proficient", since = "2022" },
    new { developerId = "d12", technologyId = "t03", proficiency = "Expert", since = "2017" }, new { developerId = "d12", technologyId = "t20", proficiency = "Advanced", since = "2019" }, new { developerId = "d12", technologyId = "t09", proficiency = "Proficient", since = "2020" }, new { developerId = "d12", technologyId = "t19", proficiency = "Advanced", since = "2020" },
    new { developerId = "d13", technologyId = "t06", proficiency = "Proficient", since = "2021" }, new { developerId = "d13", technologyId = "t08", proficiency = "Proficient", since = "2019" }, new { developerId = "d13", technologyId = "t05", proficiency = "Advanced", since = "2020" }, new { developerId = "d13", technologyId = "t10", proficiency = "Advanced", since = "2019" },
    new { developerId = "d14", technologyId = "t01", proficiency = "Proficient", since = "2017" }, new { developerId = "d14", technologyId = "t05", proficiency = "Advanced", since = "2019" }, new { developerId = "d14", technologyId = "t11", proficiency = "Proficient", since = "2021" },
    new { developerId = "d15", technologyId = "t02", proficiency = "Advanced", since = "2018" }, new { developerId = "d15", technologyId = "t04", proficiency = "Advanced", since = "2019" }, new { developerId = "d15", technologyId = "t16", proficiency = "Proficient", since = "2020" },
    new { developerId = "d16", technologyId = "t05", proficiency = "Proficient", since = "2020" }, new { developerId = "d16", technologyId = "t06", proficiency = "Proficient", since = "2021" }, new { developerId = "d16", technologyId = "t18", proficiency = "Advanced", since = "2021" },
    new { developerId = "d17", technologyId = "t01", proficiency = "Advanced", since = "2019" }, new { developerId = "d17", technologyId = "t09", proficiency = "Advanced", since = "2020" }, new { developerId = "d17", technologyId = "t24", proficiency = "Proficient", since = "2022" }, new { developerId = "d17", technologyId = "t26", proficiency = "Proficient", since = "2023" },
    new { developerId = "d18", technologyId = "t03", proficiency = "Expert", since = "2018" }, new { developerId = "d18", technologyId = "t22", proficiency = "Advanced", since = "2021" }, new { developerId = "d18", technologyId = "t20", proficiency = "Advanced", since = "2019" },
    new { developerId = "d19", technologyId = "t08", proficiency = "Advanced", since = "2020" }, new { developerId = "d19", technologyId = "t06", proficiency = "Proficient", since = "2021" }, new { developerId = "d19", technologyId = "t05", proficiency = "Advanced", since = "2020" }, new { developerId = "d19", technologyId = "t23", proficiency = "Proficient", since = "2022" },
    new { developerId = "d20", technologyId = "t05", proficiency = "Expert", since = "2020" }, new { developerId = "d20", technologyId = "t06", proficiency = "Advanced", since = "2021" }, new { developerId = "d20", technologyId = "t07", proficiency = "Advanced", since = "2021" },
    new { developerId = "d21", technologyId = "t03", proficiency = "Advanced", since = "2019" }, new { developerId = "d21", technologyId = "t23", proficiency = "Expert", since = "2021" }, new { developerId = "d21", technologyId = "t17", proficiency = "Advanced", since = "2020" }, new { developerId = "d21", technologyId = "t22", proficiency = "Proficient", since = "2022" },
    new { developerId = "d22", technologyId = "t14", proficiency = "Advanced", since = "2021" }, new { developerId = "d22", technologyId = "t13", proficiency = "Expert", since = "2020" }, new { developerId = "d22", technologyId = "t16", proficiency = "Proficient", since = "2022" },
    new { developerId = "d23", technologyId = "t05", proficiency = "Proficient", since = "2021" }, new { developerId = "d23", technologyId = "t06", proficiency = "Proficient", since = "2021" }, new { developerId = "d23", technologyId = "t18", proficiency = "Advanced", since = "2022" },
    new { developerId = "d24", technologyId = "t03", proficiency = "Advanced", since = "2018" }, new { developerId = "d24", technologyId = "t05", proficiency = "Advanced", since = "2019" }, new { developerId = "d24", technologyId = "t02", proficiency = "Proficient", since = "2020" }, new { developerId = "d24", technologyId = "t09", proficiency = "Proficient", since = "2019" },
    new { developerId = "d25", technologyId = "t26", proficiency = "Advanced", since = "2020" }, new { developerId = "d25", technologyId = "t04", proficiency = "Advanced", since = "2019" }, new { developerId = "d25", technologyId = "t16", proficiency = "Proficient", since = "2021" }, new { developerId = "d25", technologyId = "t11", proficiency = "Proficient", since = "2022" },
    new { developerId = "d26", technologyId = "t01", proficiency = "Expert", since = "2018" }, new { developerId = "d26", technologyId = "t18", proficiency = "Advanced", since = "2020" }, new { developerId = "d26", technologyId = "t24", proficiency = "Advanced", since = "2021" }, new { developerId = "d26", technologyId = "t09", proficiency = "Proficient", since = "2019" },
    new { developerId = "d27", technologyId = "t03", proficiency = "Advanced", since = "2019" }, new { developerId = "d27", technologyId = "t20", proficiency = "Advanced", since = "2020" }, new { developerId = "d27", technologyId = "t25", proficiency = "Proficient", since = "2021" }, new { developerId = "d27", technologyId = "t22", proficiency = "Proficient", since = "2022" },
    new { developerId = "d28", technologyId = "t05", proficiency = "Expert", since = "2021" }, new { developerId = "d28", technologyId = "t06", proficiency = "Advanced", since = "2022" }, new { developerId = "d28", technologyId = "t07", proficiency = "Proficient", since = "2022" },
};

public static readonly object[] ContributedTo =
{
    new { developerId = "d1", repositoryId = "r01", contributionCount = 120, since = "2021" }, new { developerId = "d1", repositoryId = "r03", contributionCount = 35, since = "2021" },
    new { developerId = "d2", repositoryId = "r02", contributionCount = 80, since = "2022" },
    new { developerId = "d3", repositoryId = "r08", contributionCount = 60, since = "2023" }, new { developerId = "d3", repositoryId = "r09", contributionCount = 50, since = "2023" },
    new { developerId = "d4", repositoryId = "r03", contributionCount = 45, since = "2021" },
    new { developerId = "d5", repositoryId = "r04", contributionCount = 90, since = "2022" },
    new { developerId = "d6", repositoryId = "r06", contributionCount = 70, since = "2022" },
    new { developerId = "d7", repositoryId = "r02", contributionCount = 20, since = "2021" }, new { developerId = "d7", repositoryId = "r08", contributionCount = 15, since = "2023" },
    new { developerId = "d8", repositoryId = "r11", contributionCount = 60, since = "2023" },
    new { developerId = "d9", repositoryId = "r05", contributionCount = 55, since = "2022" }, new { developerId = "d9", repositoryId = "r13", contributionCount = 40, since = "2024" },
    new { developerId = "d10", repositoryId = "r09", contributionCount = 45, since = "2023" }, new { developerId = "d10", repositoryId = "r14", contributionCount = 30, since = "2024" },
    new { developerId = "d11", repositoryId = "r10", contributionCount = 65, since = "2022" },
    new { developerId = "d12", repositoryId = "r12", contributionCount = 85, since = "2020" },
    new { developerId = "d13", repositoryId = "r07", contributionCount = 50, since = "2022" }, new { developerId = "d13", repositoryId = "r14", contributionCount = 40, since = "2024" },
    new { developerId = "d14", repositoryId = "r01", contributionCount = 25, since = "2021" },
    new { developerId = "d15", repositoryId = "r10", contributionCount = 40, since = "2022" },
    new { developerId = "d16", repositoryId = "r10", contributionCount = 18, since = "2022" },
    new { developerId = "d17", repositoryId = "r01", contributionCount = 60, since = "2022" }, new { developerId = "d17", repositoryId = "r16", contributionCount = 45, since = "2023" },
    new { developerId = "d18", repositoryId = "r15", contributionCount = 90, since = "2023" }, new { developerId = "d18", repositoryId = "r12", contributionCount = 30, since = "2020" },
    new { developerId = "d19", repositoryId = "r17", contributionCount = 55, since = "2024" }, new { developerId = "d19", repositoryId = "r14", contributionCount = 35, since = "2024" },
    new { developerId = "d20", repositoryId = "r17", contributionCount = 40, since = "2024" }, new { developerId = "d20", repositoryId = "r13", contributionCount = 25, since = "2024" },
    new { developerId = "d21", repositoryId = "r15", contributionCount = 50, since = "2023" }, new { developerId = "d21", repositoryId = "r04", contributionCount = 25, since = "2023" },
    new { developerId = "d22", repositoryId = "r18", contributionCount = 45, since = "2023" }, new { developerId = "d22", repositoryId = "r16", contributionCount = 30, since = "2023" },
    new { developerId = "d23", repositoryId = "r18", contributionCount = 20, since = "2023" }, new { developerId = "d23", repositoryId = "r20", contributionCount = 15, since = "2024" },
    new { developerId = "d24", repositoryId = "r19", contributionCount = 65, since = "2022" }, new { developerId = "d24", repositoryId = "r01", contributionCount = 25, since = "2022" },
    new { developerId = "d25", repositoryId = "r18", contributionCount = 35, since = "2023" }, new { developerId = "d25", repositoryId = "r10", contributionCount = 25, since = "2023" },
    new { developerId = "d26", repositoryId = "r20", contributionCount = 70, since = "2024" }, new { developerId = "d26", repositoryId = "r18", contributionCount = 30, since = "2023" },
    new { developerId = "d27", repositoryId = "r15", contributionCount = 55, since = "2023" }, new { developerId = "d27", repositoryId = "r19", contributionCount = 25, since = "2024" },
    new { developerId = "d28", repositoryId = "r16", contributionCount = 45, since = "2023" }, new { developerId = "d28", repositoryId = "r21", contributionCount = 20, since = "2024" },
};

public static readonly object[] RequiresSkill =
{
    new { taskId = "sk1", technologyId = "t01" }, new { taskId = "sk1", technologyId = "t09" },
    new { taskId = "sk2", technologyId = "t01" }, new { taskId = "sk2", technologyId = "t05" },
    new { taskId = "sk3", technologyId = "t09" },
    new { taskId = "sk4", technologyId = "t01" }, new { taskId = "sk4", technologyId = "t05" },
    new { taskId = "sk5", technologyId = "t03" }, new { taskId = "sk5", technologyId = "t05" },
    new { taskId = "sk6", technologyId = "t05" }, new { taskId = "sk6", technologyId = "t06" },
    new { taskId = "sk7", technologyId = "t19" }, new { taskId = "sk7", technologyId = "t20" },
    new { taskId = "sk8", technologyId = "t03" }, new { taskId = "sk8", technologyId = "t09" },
    new { taskId = "sk9", technologyId = "t08" }, new { taskId = "sk9", technologyId = "t10" },
    new { taskId = "sk10", technologyId = "t08" }, new { taskId = "sk10", technologyId = "t12" },
    new { taskId = "sk11", technologyId = "t05" }, new { taskId = "sk11", technologyId = "t06" },
    new { taskId = "sk12", technologyId = "t01" }, new { taskId = "sk12", technologyId = "t06" },
    new { taskId = "sk13", technologyId = "t01" }, new { taskId = "sk13", technologyId = "t09" },
    new { taskId = "sk14", technologyId = "t05" }, new { taskId = "sk14", technologyId = "t06" },
    new { taskId = "sk15", technologyId = "t02" }, new { taskId = "sk15", technologyId = "t04" },
    new { taskId = "sk16", technologyId = "t04" }, new { taskId = "sk16", technologyId = "t16" },
    new { taskId = "sk17", technologyId = "t05" },
    new { taskId = "sk18", technologyId = "t08" }, new { taskId = "sk18", technologyId = "t11" },
    new { taskId = "sk19", technologyId = "t12" },
    new { taskId = "sk20", technologyId = "t11" }, new { taskId = "sk20", technologyId = "t12" },
    new { taskId = "sk21", technologyId = "t08" },
    new { taskId = "sk22", technologyId = "t03" }, new { taskId = "sk22", technologyId = "t19" },
    new { taskId = "sk23", technologyId = "t03" }, new { taskId = "sk23", technologyId = "t20" },
    new { taskId = "sk24", technologyId = "t19" }, new { taskId = "sk24", technologyId = "t15" },
    new { taskId = "sk25", technologyId = "t08" }, new { taskId = "sk25", technologyId = "t18" },
    new { taskId = "sk26", technologyId = "t11" }, new { taskId = "sk26", technologyId = "t05" },
    new { taskId = "sk27", technologyId = "t08" }, new { taskId = "sk27", technologyId = "t18" },
    new { taskId = "sk28", technologyId = "t12" }, new { taskId = "sk28", technologyId = "t04" },
    new { taskId = "sk29", technologyId = "t04" }, new { taskId = "sk29", technologyId = "t03" },
    new { taskId = "sk30", technologyId = "t05" }, new { taskId = "sk30", technologyId = "t11" },
    new { taskId = "sk31", technologyId = "t03" }, new { taskId = "sk31", technologyId = "t17" },
    new { taskId = "sk32", technologyId = "t20" }, new { taskId = "sk32", technologyId = "t22" },
    new { taskId = "sk33", technologyId = "t20" }, new { taskId = "sk33", technologyId = "t25" },
    new { taskId = "sk34", technologyId = "t03" }, new { taskId = "sk34", technologyId = "t25" },
    new { taskId = "sk35", technologyId = "t01" }, new { taskId = "sk35", technologyId = "t13" },
    new { taskId = "sk36", technologyId = "t13" }, new { taskId = "sk36", technologyId = "t14" },
    new { taskId = "sk37", technologyId = "t05" }, new { taskId = "sk37", technologyId = "t16" },
    new { taskId = "sk38", technologyId = "t08" }, new { taskId = "sk38", technologyId = "t23" },
    new { taskId = "sk39", technologyId = "t06" }, new { taskId = "sk39", technologyId = "t05" },
    new { taskId = "sk40", technologyId = "t01" }, new { taskId = "sk40", technologyId = "t18" },
    new { taskId = "sk41", technologyId = "t01" }, new { taskId = "sk41", technologyId = "t12" },
    new { taskId = "sk42", technologyId = "t05" }, new { taskId = "sk42", technologyId = "t18" },
    new { taskId = "sk43", technologyId = "t02" }, new { taskId = "sk43", technologyId = "t21" },
    new { taskId = "sk44", technologyId = "t02" }, new { taskId = "sk44", technologyId = "t18" },
    new { taskId = "sk45", technologyId = "t26" }, new { taskId = "sk45", technologyId = "t13" },
    new { taskId = "sk46", technologyId = "t26" }, new { taskId = "sk46", technologyId = "t24" },
};
}
