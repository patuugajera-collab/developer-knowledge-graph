using DeveloperKnowledgeGraph.Api.Data;
using DeveloperKnowledgeGraph.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

var connectionString = ResolveConnectionString(args);

Console.WriteLine($"Connecting to SQL Server ...");

await using var db = CreateContext(connectionString);

try
{
    await db.Database.CanConnectAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unable to connect to the database: {ex.Message}");
    return 1;
}

Console.WriteLine("Connected to SQL Server.\n");

Console.WriteLine("Recreating database schema ...");
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
Console.WriteLine("Schema ready.\n");

Seed(db);

await db.SaveChangesAsync();

Console.WriteLine("Seed completed successfully.");
return 0;

static string ResolveConnectionString(string[] args)
{
    if (args.Length > 0)
    {
        return args[0];
    }

    var env = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
    if (!string.IsNullOrWhiteSpace(env))
    {
        return env!;
    }

    return "Server=(localdb)\\MSSQLLocalDB;Database=DeveloperKnowledgeGraph;Trusted_Connection=True;TrustServerCertificate=True;";
}

static AppDbContext CreateContext(string connectionString)
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    return new AppDbContext(options);
}

static void Seed(AppDbContext db)
{
    SeedOrganizations(db);
    SeedDevelopers(db);
    SeedProjects(db);
    SeedTechnologies(db);
    SeedRepositories(db);
    SeedTasks(db);
    SeedWorksFor(db);
    SeedOwns(db);
    SeedWorksOn(db);
    SeedUses(db);
    SeedDependsOn(db);
    SeedHasSkill(db);
    SeedContributedTo(db);
    SeedRequiresSkill(db);
}

static void SeedOrganizations(AppDbContext db)
{
    var data = new[]
    {
        (Id: "o1", Name: "Acme Corp"),
        (Id: "o2", Name: "Nimbus Labs"),
        (Id: "o3", Name: "Helix Industries"),
        (Id: "o4", Name: "Beacon Soft"),
        (Id: "o5", Name: "OpenVox"),
        (Id: "o6", Name: "Cobalt Systems"),
        (Id: "o7", Name: "Vertex Robotics"),
        (Id: "o8", Name: "Zephyr Media"),
        (Id: "o9", Name: "Ironclad Logistics"),
    };

    foreach (var row in data)
    {
        db.Organizations.Add(new Organization { Id = row.Id, Name = row.Name });
    }
}

static void SeedDevelopers(AppDbContext db)
{
    var data = new[]
    {
        (Id: "d1", Name: "Alice Chen", Email: "alice.chen@example.com", Role: "Backend Engineer"),
        (Id: "d2", Name: "Marcus Johnson", Email: "marcus.johnson@example.com", Role: "Frontend Engineer"),
        (Id: "d3", Name: "Priya Sharma", Email: "priya.sharma@example.com", Role: "Full-Stack Engineer"),
        (Id: "d4", Name: "Diego Martinez", Email: "diego.martinez@example.com", Role: "DevOps Engineer"),
        (Id: "d5", Name: "Emily Watson", Email: "emily.watson@example.com", Role: "Data Engineer"),
        (Id: "d6", Name: "Omar Farouk", Email: "omar.farouk@example.com", Role: "ML Engineer"),
        (Id: "d7", Name: "Sofia Rossi", Email: "sofia.rossi@example.com", Role: "QA Engineer"),
        (Id: "d8", Name: "Liam O'Brien", Email: "liam.obrien@example.com", Role: "Backend Engineer"),
        (Id: "d9", Name: "Yuki Tanaka", Email: "yuki.tanaka@example.com", Role: "Frontend Engineer"),
        (Id: "d10", Name: "Aisha Bello", Email: "aisha.bello@example.com", Role: "Product Engineer"),
        (Id: "d11", Name: "Noah Kim", Email: "noah.kim@example.com", Role: "DevOps Engineer"),
        (Id: "d12", Name: "Freya Novak", Email: "freya.novak@example.com", Role: "Data Engineer"),
        (Id: "d13", Name: "Kenji Sato", Email: "kenji.sato@example.com", Role: "Mobile Engineer"),
        (Id: "d14", Name: "Lucia Blanco", Email: "lucia.blanco@example.com", Role: "Engineering Manager"),
        (Id: "d15", Name: "Tariq Haddad", Email: "tariq.haddad@example.com", Role: "Security Engineer"),
        (Id: "d16", Name: "Emma Larsson", Email: "emma.larsson@example.com", Role: "QA Engineer"),
        (Id: "d17", Name: "Devin Carter", Email: "devin.carter@example.com", Role: "Backend Engineer"),
        (Id: "d18", Name: "Nina Petrov", Email: "nina.petrov@example.com", Role: "Data Engineer"),
        (Id: "d19", Name: "Ravi Menon", Email: "ravi.menon@example.com", Role: "Mobile Engineer"),
        (Id: "d20", Name: "Hannah Weiss", Email: "hannah.weiss@example.com", Role: "Frontend Engineer"),
        (Id: "d21", Name: "Tomás Silva", Email: "tomas.silva@example.com", Role: "ML Engineer"),
        (Id: "d22", Name: "Zara Haddad", Email: "zara.haddad@example.com", Role: "DevOps Engineer"),
        (Id: "d23", Name: "Elena Moreau", Email: "elena.moreau@example.com", Role: "QA Engineer"),
        (Id: "d24", Name: "Samir Nasser", Email: "samir.nasser@example.com", Role: "Full-Stack Engineer"),
        (Id: "d25", Name: "Ingrid Sørensen", Email: "ingrid.sorensen@example.com", Role: "Security Engineer"),
        (Id: "d26", Name: "Petra Klein", Email: "petra.klein@example.com", Role: "Backend Engineer"),
        (Id: "d27", Name: "Jonas Lindqvist", Email: "jonas.lindqvist@example.com", Role: "Data Engineer"),
        (Id: "d28", Name: "Amara Okafor", Email: "amara.okafor@example.com", Role: "Frontend Engineer"),
    };

    foreach (var row in data)
    {
        db.Developers.Add(new Developer { Id = row.Id, Name = row.Name, Email = row.Email, Role = row.Role });
    }
}

static void SeedProjects(AppDbContext db)
{
    var data = new[]
    {
        (Id: "p1", Name: "Atlas ERP", Description: "Enterprise resource planning suite for mid-size organisations.", Status: "active"),
        (Id: "p2", Name: "Nova Analytics Platform", Description: "Self-service BI and analytics over streaming and batch data.", Status: "active"),
        (Id: "p3", Name: "Pulse Health Monitor", Description: "Real-time patient vitals monitoring with alerting.", Status: "active"),
        (Id: "p4", Name: "Quantum Commerce", Description: "High-volume e-commerce storefront and checkout platform.", Status: "in_progress"),
        (Id: "p5", Name: "Sentinel Security Suite", Description: "Vulnerability scanning and threat intelligence platform.", Status: "active"),
        (Id: "p6", Name: "Orbit Task Scheduler", Description: "Distributed, dependency-aware job scheduler.", Status: "in_progress"),
        (Id: "p7", Name: "Flux Data Pipeline", Description: "Replayable stream ingestion and enrichment pipeline.", Status: "maintenance"),
        (Id: "p8", Name: "Helios Chat Platform", Description: "Real-time chat and presence platform with bot integrations.", Status: "planning"),
        (Id: "p9", Name: "Zenith Mobile Wallet", Description: "Digital wallet with P2P transfers and fraud detection.", Status: "in_progress"),
        (Id: "p10", Name: "Aurora Data Lake", Description: "Centralised data lake with governed access and lineage.", Status: "active"),
        (Id: "p11", Name: "Cobalt CI/CD", Description: "Continuous integration and delivery pipelines as a service.", Status: "active"),
        (Id: "p12", Name: "Vega Fitness Tracker", Description: "Wearable fitness tracking with social challenges.", Status: "planning"),
        (Id: "p13", Name: "Aegis Identity", Description: "Central identity and access management platform.", Status: "in_progress"),
        (Id: "p14", Name: "Boreal EDI", Description: "Electronic data interchange gateway for logistics.", Status: "maintenance"),
        (Id: "p15", Name: "Stratos Edge Mesh", Description: "Edge computing mesh for low-latency IoT workloads.", Status: "in_progress"),
    };

    foreach (var row in data)
    {
        db.Projects.Add(new Project { Id = row.Id, Name = row.Name, Description = row.Description, Status = row.Status });
    }
}

static void SeedTechnologies(AppDbContext db)
{
    var data = new[]
    {
        (Id: "t01", Name: ".NET / C#", Category: "Backend"),
        (Id: "t02", Name: "Java", Category: "Backend"),
        (Id: "t03", Name: "Python", Category: "Backend"),
        (Id: "t04", Name: "Go", Category: "Backend"),
        (Id: "t05", Name: "TypeScript", Category: "Frontend"),
        (Id: "t06", Name: "React", Category: "Frontend"),
        (Id: "t07", Name: "Angular", Category: "Frontend"),
        (Id: "t08", Name: "Node.js", Category: "Backend"),
        (Id: "t09", Name: "PostgreSQL", Category: "Database"),
        (Id: "t10", Name: "MongoDB", Category: "Database"),
        (Id: "t11", Name: "Neo4j", Category: "Database"),
        (Id: "t12", Name: "Redis", Category: "Infrastructure"),
        (Id: "t13", Name: "Docker", Category: "DevOps"),
        (Id: "t14", Name: "Kubernetes", Category: "DevOps"),
        (Id: "t15", Name: "AWS", Category: "Cloud"),
        (Id: "t16", Name: "Azure", Category: "Cloud"),
        (Id: "t17", Name: "TensorFlow", Category: "ML"),
        (Id: "t18", Name: "GraphQL", Category: "API"),
        (Id: "t19", Name: "Apache Kafka", Category: "Streaming"),
        (Id: "t20", Name: "Apache Spark", Category: "Big Data"),
        (Id: "t21", Name: "RabbitMQ", Category: "Infrastructure"),
        (Id: "t22", Name: "Flink", Category: "Streaming"),
        (Id: "t23", Name: "PyTorch", Category: "ML"),
        (Id: "t24", Name: "gRPC", Category: "API"),
        (Id: "t25", Name: "Cassandra", Category: "Database"),
        (Id: "t26", Name: "Rust", Category: "Backend"),
    };

    foreach (var row in data)
    {
        db.Technologies.Add(new Technology { Id = row.Id, Name = row.Name, Category = row.Category });
    }
}

static void SeedRepositories(AppDbContext db)
{
    var data = new[]
    {
        (Id: "r01", Name: "atlas-api", Url: "https://github.com/acme/atlas-api", ProjectId: "p1"),
        (Id: "r02", Name: "atlas-web", Url: "https://github.com/acme/atlas-web", ProjectId: "p1"),
        (Id: "r03", Name: "atlas-infra", Url: "https://github.com/acme/atlas-infra", ProjectId: "p1"),
        (Id: "r04", Name: "nova-query-engine", Url: "https://github.com/acme/nova-query-engine", ProjectId: "p2"),
        (Id: "r05", Name: "nova-dashboard", Url: "https://github.com/acme/nova-dashboard", ProjectId: "p2"),
        (Id: "r06", Name: "pulse-backend", Url: "https://github.com/beacon/pulse-backend", ProjectId: "p3"),
        (Id: "r07", Name: "pulse-mobile", Url: "https://github.com/beacon/pulse-mobile", ProjectId: "p3"),
        (Id: "r08", Name: "quantum-storefront", Url: "https://github.com/nimbus/quantum-storefront", ProjectId: "p4"),
        (Id: "r09", Name: "quantum-checkout", Url: "https://github.com/nimbus/quantum-checkout", ProjectId: "p4"),
        (Id: "r10", Name: "sentinel-scanner", Url: "https://github.com/openvox/sentinel-scanner", ProjectId: "p5"),
        (Id: "r11", Name: "orbit-scheduler", Url: "https://github.com/helix/orbit-scheduler", ProjectId: "p6"),
        (Id: "r12", Name: "flux-pipeline", Url: "https://github.com/acme/flux-pipeline", ProjectId: "p7"),
        (Id: "r13", Name: "helios-gateway", Url: "https://github.com/beacon/helios-gateway", ProjectId: "p8"),
        (Id: "r14", Name: "zenith-wallet-app", Url: "https://github.com/nimbus/zenith-wallet-app", ProjectId: "p9"),
        (Id: "r15", Name: "aurora-lake", Url: "https://github.com/cobalt/aurora-lake", ProjectId: "p10"),
        (Id: "r16", Name: "cobalt-cicd", Url: "https://github.com/cobalt/cobalt-cicd", ProjectId: "p11"),
        (Id: "r17", Name: "vega-tracker-app", Url: "https://github.com/vertex/vega-tracker-app", ProjectId: "p12"),
        (Id: "r18", Name: "aegis-idp", Url: "https://github.com/openvox/aegis-idp", ProjectId: "p13"),
        (Id: "r19", Name: "boreal-edi-gateway", Url: "https://github.com/ironclad/boreal-edi-gateway", ProjectId: "p14"),
        (Id: "r20", Name: "stratos-edge-mesh", Url: "https://github.com/vertex/stratos-edge-mesh", ProjectId: "p15"),
        (Id: "r21", Name: "zephyr-media-api", Url: "https://github.com/zephyr/zephyr-media-api", ProjectId: "p12"),
        (Id: "r22", Name: "cobalt-observability", Url: "https://github.com/cobalt/cobalt-observability", ProjectId: "p11"),
    };

    foreach (var row in data)
    {
        db.Repositories.Add(new Repository { Id = row.Id, Name = row.Name, Url = row.Url, ProjectId = row.ProjectId });
    }
}

static void SeedTasks(AppDbContext db)
{
    var data = new[]
    {
        (Id: "sk1", Title: "Build inventory ledger API", Status: "done", Priority: 1, ProjectId: "p1"),
        (Id: "sk2", Title: "Implement OAuth2 SSO", Status: "in_progress", Priority: 2, ProjectId: "p1"),
        (Id: "sk3", Title: "Migrate customer records", Status: "todo", Priority: 3, ProjectId: "p1"),
        (Id: "sk4", Title: "Add reporting endpoints", Status: "backlog", Priority: 4, ProjectId: "p1"),
        (Id: "sk5", Title: "Design query engine DSL", Status: "done", Priority: 1, ProjectId: "p2"),
        (Id: "sk6", Title: "Build dashboard widgets", Status: "in_progress", Priority: 2, ProjectId: "p2"),
        (Id: "sk7", Title: "Connect streaming ingestion", Status: "todo", Priority: 2, ProjectId: "p2"),
        (Id: "sk8", Title: "Alerting rules engine", Status: "backlog", Priority: 3, ProjectId: "p2"),
        (Id: "sk9", Title: "Implement vitals ingestion", Status: "done", Priority: 1, ProjectId: "p3"),
        (Id: "sk10", Title: "Build alerting pipeline", Status: "in_progress", Priority: 2, ProjectId: "p3"),
        (Id: "sk11", Title: "Mobile push notifications", Status: "todo", Priority: 3, ProjectId: "p3"),
        (Id: "sk12", Title: "Cart checkout flow", Status: "in_progress", Priority: 1, ProjectId: "p4"),
        (Id: "sk13", Title: "Payment gateway integration", Status: "todo", Priority: 1, ProjectId: "p4"),
        (Id: "sk14", Title: "Product catalog search", Status: "done", Priority: 2, ProjectId: "p4"),
        (Id: "sk15", Title: "Build vulnerability scanner", Status: "done", Priority: 1, ProjectId: "p5"),
        (Id: "sk16", Title: "Connect threat intel feed", Status: "in_progress", Priority: 2, ProjectId: "p5"),
        (Id: "sk17", Title: "Generate compliance reports", Status: "todo", Priority: 3, ProjectId: "p5"),
        (Id: "sk18", Title: "Scheduler core loop", Status: "in_progress", Priority: 1, ProjectId: "p6"),
        (Id: "sk19", Title: "Retry policy engine", Status: "todo", Priority: 2, ProjectId: "p6"),
        (Id: "sk20", Title: "Service integration hooks", Status: "backlog", Priority: 3, ProjectId: "p6"),
        (Id: "sk21", Title: "Audit log exporter", Status: "backlog", Priority: 4, ProjectId: "p6"),
        (Id: "sk22", Title: "Replayable pipeline stages", Status: "done", Priority: 1, ProjectId: "p7"),
        (Id: "sk23", Title: "Schema registry", Status: "in_progress", Priority: 2, ProjectId: "p7"),
        (Id: "sk24", Title: "Dead letter queue", Status: "todo", Priority: 3, ProjectId: "p7"),
        (Id: "sk25", Title: "Chat message routing", Status: "todo", Priority: 1, ProjectId: "p8"),
        (Id: "sk26", Title: "Real-time presence", Status: "backlog", Priority: 2, ProjectId: "p8"),
        (Id: "sk27", Title: "Bot framework integration", Status: "backlog", Priority: 3, ProjectId: "p8"),
        (Id: "sk28", Title: "Wallet balance cache", Status: "in_progress", Priority: 1, ProjectId: "p9"),
        (Id: "sk29", Title: "P2P transfers", Status: "todo", Priority: 1, ProjectId: "p9"),
        (Id: "sk30", Title: "KYC verification", Status: "todo", Priority: 2, ProjectId: "p9"),
        (Id: "sk31", Title: "Fraud detection rules", Status: "backlog", Priority: 3, ProjectId: "p9"),
        (Id: "sk32", Title: "Lake ingestion pipelines", Status: "in_progress", Priority: 1, ProjectId: "p10"),
        (Id: "sk33", Title: "Columnar tables & partitioning", Status: "todo", Priority: 2, ProjectId: "p10"),
        (Id: "sk34", Title: "Data lineage catalog", Status: "backlog", Priority: 3, ProjectId: "p10"),
        (Id: "sk35", Title: "Pipeline orchestrator service", Status: "in_progress", Priority: 1, ProjectId: "p11"),
        (Id: "sk36", Title: "Artifact registry", Status: "todo", Priority: 2, ProjectId: "p11"),
        (Id: "sk37", Title: "Observability dashboards", Status: "done", Priority: 3, ProjectId: "p11"),
        (Id: "sk38", Title: "Sensor data ingestion", Status: "todo", Priority: 1, ProjectId: "p12"),
        (Id: "sk39", Title: "Activity rings UI", Status: "backlog", Priority: 2, ProjectId: "p12"),
        (Id: "sk40", Title: "SSO integration", Status: "in_progress", Priority: 1, ProjectId: "p13"),
        (Id: "sk41", Title: "MFA enforcement", Status: "todo", Priority: 1, ProjectId: "p13"),
        (Id: "sk42", Title: "Role-based access controls", Status: "done", Priority: 2, ProjectId: "p13"),
        (Id: "sk43", Title: "EDI message transformation", Status: "done", Priority: 1, ProjectId: "p14"),
        (Id: "sk44", Title: "Partner onboarding API", Status: "in_progress", Priority: 2, ProjectId: "p14"),
        (Id: "sk45", Title: "Edge node deployment", Status: "todo", Priority: 1, ProjectId: "p15"),
        (Id: "sk46", Title: "Low-latency sync protocol", Status: "backlog", Priority: 2, ProjectId: "p15"),
    };

    foreach (var row in data)
    {
        db.Tasks.Add(new WorkTask { Id = row.Id, Title = row.Title, Status = row.Status, Priority = row.Priority, ProjectId = row.ProjectId });
    }
}

static void SeedWorksFor(AppDbContext db)
{
    var data = new[]
    {
        ("d1", "o1"), ("d2", "o1"), ("d5", "o1"), ("d9", "o1"), ("d14", "o1"),
        ("d3", "o2"), ("d10", "o2"), ("d13", "o2"),
        ("d4", "o3"), ("d8", "o3"), ("d11", "o3"),
        ("d6", "o4"), ("d12", "o4"), ("d16", "o4"),
        ("d7", "o5"), ("d15", "o5"),
        ("d17", "o6"), ("d20", "o6"), ("d24", "o6"), ("d28", "o6"),
        ("d18", "o7"), ("d19", "o7"), ("d21", "o7"),
        ("d22", "o8"), ("d23", "o8"),
        ("d25", "o9"), ("d26", "o9"), ("d27", "o9"),
    };

    foreach (var (developerId, organizationId) in data)
    {
        db.WorksForRelations.Add(new WorksForEdge { DeveloperId = developerId, OrganizationId = organizationId, Since = "2020" });
    }
}

static void SeedOwns(AppDbContext db)
{
    var data = new[]
    {
        ("o1", "p1"), ("o1", "p2"), ("o1", "p7"),
        ("o2", "p4"), ("o2", "p9"),
        ("o3", "p6"),
        ("o4", "p3"), ("o4", "p8"),
        ("o5", "p5"), ("o5", "p13"),
        ("o6", "p10"), ("o6", "p11"),
        ("o7", "p12"), ("o7", "p15"),
        ("o8", "p12"),
        ("o9", "p14"),
    };

    foreach (var (organizationId, projectId) in data)
    {
        db.OwnsRelations.Add(new OwnsEdge { OrganizationId = organizationId, ProjectId = projectId });
    }
}

static void SeedWorksOn(AppDbContext db)
{
    var data = new[]
    {
        ("d1", "p1", "Lead", "2021"), ("d1", "p6", "Contributor", "2023"),
        ("d2", "p2", "Lead", "2022"), ("d2", "p8", "Contributor", "2024"),
        ("d3", "p4", "Lead", "2023"), ("d3", "p1", "Contributor", "2021"),
        ("d4", "p1", "Infra", "2021"), ("d4", "p5", "Member", "2022"),
        ("d5", "p2", "Lead", "2022"), ("d5", "p7", "Contributor", "2020"),
        ("d6", "p3", "Lead", "2022"), ("d6", "p8", "Contributor", "2024"),
        ("d7", "p1", "QA", "2021"), ("d7", "p4", "QA", "2023"),
        ("d8", "p6", "Lead", "2023"), ("d8", "p5", "Contributor", "2022"),
        ("d9", "p8", "Lead", "2024"), ("d9", "p2", "Contributor", "2022"),
        ("d10", "p4", "Product", "2023"), ("d10", "p9", "Lead", "2024"),
        ("d11", "p5", "DevOps", "2022"), ("d11", "p2", "Contributor", "2023"),
        ("d12", "p7", "Lead", "2020"), ("d12", "p9", "Contributor", "2024"),
        ("d13", "p9", "Mobile", "2024"), ("d13", "p3", "Contributor", "2022"),
        ("d14", "p1", "Manager", "2021"), ("d14", "p3", "Manager", "2022"),
        ("d15", "p5", "Security", "2022"), ("d15", "p1", "Consultant", "2021"),
        ("d16", "p5", "QA", "2022"), ("d16", "p9", "QA", "2024"),
        ("d17", "p1", "Contributor", "2022"), ("d17", "p11", "Lead", "2023"),
        ("d18", "p10", "Lead", "2023"), ("d18", "p7", "Contributor", "2020"),
        ("d19", "p12", "Mobile", "2024"), ("d19", "p9", "Contributor", "2024"),
        ("d20", "p8", "Contributor", "2024"), ("d20", "p12", "Frontend", "2024"),
        ("d21", "p2", "ML", "2023"), ("d21", "p10", "Contributor", "2023"),
        ("d22", "p13", "DevOps", "2023"), ("d22", "p11", "Contributor", "2023"),
        ("d23", "p13", "QA", "2023"), ("d23", "p15", "QA", "2024"),
        ("d24", "p14", "Lead", "2022"), ("d24", "p1", "Contributor", "2022"),
        ("d25", "p13", "Security", "2023"), ("d25", "p5", "Consultant", "2023"),
        ("d26", "p15", "Lead", "2024"), ("d26", "p13", "Contributor", "2023"),
        ("d27", "p10", "Data", "2023"), ("d27", "p14", "Contributor", "2024"),
        ("d28", "p11", "Frontend", "2023"), ("d28", "p15", "Contributor", "2024"),
    };

    foreach (var (developerId, projectId, role, since) in data)
    {
        db.WorksOnRelations.Add(new WorksOnEdge { DeveloperId = developerId, ProjectId = projectId, Role = role, Since = since });
    }
}

static void SeedUses(AppDbContext db)
{
    var data = new[]
    {
        ("p1", "t01", "build"), ("p1", "t05", "frontend"), ("p1", "t09", "data"), ("p1", "t13", "deployment"),
        ("p2", "t03", "processing"), ("p2", "t19", "streaming"), ("p2", "t20", "batch"), ("p2", "t05", "frontend"),
        ("p3", "t08", "runtime"), ("p3", "t10", "storage"), ("p3", "t12", "caching"), ("p3", "t17", "prediction"),
        ("p4", "t01", "build"), ("p4", "t06", "frontend"), ("p4", "t09", "data"), ("p4", "t12", "caching"),
        ("p5", "t02", "build"), ("p5", "t04", "scanner"), ("p5", "t16", "cloud"),
        ("p6", "t08", "runtime"), ("p6", "t11", "dependencies"), ("p6", "t12", "queueing"),
        ("p7", "t03", "processing"), ("p7", "t19", "streaming"), ("p7", "t20", "batch"), ("p7", "t15", "cloud"),
        ("p8", "t05", "build"), ("p8", "t08", "runtime"), ("p8", "t11", "presence"), ("p8", "t18", "api"),
        ("p9", "t04", "api"), ("p9", "t03", "services"), ("p9", "t14", "infrastructure"), ("p9", "t15", "cloud"),
        ("p1", "t24", "api"), ("p3", "t21", "messaging"), ("p4", "t15", "cloud"), ("p5", "t25", "data"),
        ("p6", "t21", "queueing"), ("p8", "t24", "api"),
        ("p10", "t20", "batch"), ("p10", "t22", "streaming"), ("p10", "t25", "storage"), ("p10", "t15", "cloud"),
        ("p11", "t13", "containers"), ("p11", "t14", "orchestration"), ("p11", "t01", "build"), ("p11", "t05", "frontend"),
        ("p12", "t08", "runtime"), ("p12", "t06", "frontend"), ("p12", "t10", "storage"), ("p12", "t23", "prediction"),
        ("p13", "t05", "frontend"), ("p13", "t01", "services"), ("p13", "t12", "sessions"), ("p13", "t18", "api"),
        ("p14", "t02", "gateway"), ("p14", "t21", "messaging"), ("p14", "t09", "storage"), ("p14", "t15", "cloud"),
        ("p15", "t26", "runtime"), ("p15", "t24", "mesh"), ("p15", "t13", "deployment"), ("p15", "t16", "cloud"),
    };

    foreach (var (projectId, technologyId, purpose) in data)
    {
        db.UsesRelations.Add(new UsesEdge { ProjectId = projectId, TechnologyId = technologyId, Purpose = purpose });
    }
}

static void SeedDependsOn(AppDbContext db)
{
    var data = new[]
    {
        ("p2", "p7"), ("p2", "p1"),
        ("p3", "p1"),
        ("p4", "p1"),
        ("p5", "p1"),
        ("p6", "p1"),
        ("p7", "p1"),
        ("p8", "p6"),
        ("p9", "p4"), ("p9", "p8"),
        ("p10", "p7"), ("p10", "p2"),
        ("p11", "p6"), ("p11", "p1"),
        ("p12", "p8"),
        ("p13", "p1"), ("p13", "p5"),
        ("p14", "p7"),
        ("p15", "p13"), ("p15", "p11"),
    };

    foreach (var (projectId, dependencyProjectId) in data)
    {
        db.DependsOnRelations.Add(new DependsOnEdge { ProjectId = projectId, DependencyProjectId = dependencyProjectId });
    }
}

static void SeedHasSkill(AppDbContext db)
{
    var data = new[]
    {
        ("d1", "t01", "Expert", "2018"), ("d1", "t09", "Advanced", "2019"), ("d1", "t05", "Proficient", "2020"), ("d1", "t11", "Advanced", "2021"),
        ("d2", "t05", "Expert", "2019"), ("d2", "t06", "Advanced", "2020"), ("d2", "t07", "Advanced", "2019"), ("d2", "t18", "Proficient", "2021"),
        ("d3", "t03", "Advanced", "2018"), ("d3", "t05", "Advanced", "2019"), ("d3", "t01", "Proficient", "2020"), ("d3", "t09", "Proficient", "2020"),
        ("d4", "t13", "Advanced", "2019"), ("d4", "t14", "Expert", "2020"), ("d4", "t15", "Proficient", "2021"),
        ("d5", "t03", "Expert", "2018"), ("d5", "t20", "Advanced", "2019"), ("d5", "t19", "Proficient", "2021"), ("d5", "t09", "Advanced", "2018"),
        ("d6", "t03", "Advanced", "2018"), ("d6", "t17", "Expert", "2019"), ("d6", "t20", "Proficient", "2020"),
        ("d7", "t05", "Proficient", "2020"), ("d7", "t06", "Advanced", "2021"),
        ("d8", "t01", "Advanced", "2018"), ("d8", "t08", "Proficient", "2020"), ("d8", "t09", "Advanced", "2019"), ("d8", "t11", "Proficient", "2021"),
        ("d9", "t05", "Expert", "2018"), ("d9", "t07", "Expert", "2019"), ("d9", "t06", "Advanced", "2020"), ("d9", "t18", "Advanced", "2021"),
        ("d10", "t03", "Proficient", "2020"), ("d10", "t05", "Expert", "2018"), ("d10", "t11", "Proficient", "2021"), ("d10", "t18", "Advanced", "2019"),
        ("d11", "t14", "Advanced", "2020"), ("d11", "t13", "Advanced", "2021"), ("d11", "t15", "Expert", "2019"), ("d11", "t04", "Proficient", "2022"),
        ("d12", "t03", "Expert", "2017"), ("d12", "t20", "Advanced", "2019"), ("d12", "t09", "Proficient", "2020"), ("d12", "t19", "Advanced", "2020"),
        ("d13", "t06", "Proficient", "2021"), ("d13", "t08", "Proficient", "2019"), ("d13", "t05", "Advanced", "2020"), ("d13", "t10", "Advanced", "2019"),
        ("d14", "t01", "Proficient", "2017"), ("d14", "t05", "Advanced", "2019"), ("d14", "t11", "Proficient", "2021"),
        ("d15", "t02", "Advanced", "2018"), ("d15", "t04", "Advanced", "2019"), ("d15", "t16", "Proficient", "2020"),
        ("d16", "t05", "Proficient", "2020"), ("d16", "t06", "Proficient", "2021"), ("d16", "t18", "Advanced", "2021"),
        ("d17", "t01", "Advanced", "2019"), ("d17", "t09", "Advanced", "2020"), ("d17", "t24", "Proficient", "2022"), ("d17", "t26", "Proficient", "2023"),
        ("d18", "t03", "Expert", "2018"), ("d18", "t22", "Advanced", "2021"), ("d18", "t20", "Advanced", "2019"),
        ("d19", "t08", "Advanced", "2020"), ("d19", "t06", "Proficient", "2021"), ("d19", "t05", "Advanced", "2020"), ("d19", "t23", "Proficient", "2022"),
        ("d20", "t05", "Expert", "2020"), ("d20", "t06", "Advanced", "2021"), ("d20", "t07", "Advanced", "2021"),
        ("d21", "t03", "Advanced", "2019"), ("d21", "t23", "Expert", "2021"), ("d21", "t17", "Advanced", "2020"), ("d21", "t22", "Proficient", "2022"),
        ("d22", "t14", "Advanced", "2021"), ("d22", "t13", "Expert", "2020"), ("d22", "t16", "Proficient", "2022"),
        ("d23", "t05", "Proficient", "2021"), ("d23", "t06", "Proficient", "2021"), ("d23", "t18", "Advanced", "2022"),
        ("d24", "t03", "Advanced", "2018"), ("d24", "t05", "Advanced", "2019"), ("d24", "t02", "Proficient", "2020"), ("d24", "t09", "Proficient", "2019"),
        ("d25", "t26", "Advanced", "2020"), ("d25", "t04", "Advanced", "2019"), ("d25", "t16", "Proficient", "2021"), ("d25", "t11", "Proficient", "2022"),
        ("d26", "t01", "Expert", "2018"), ("d26", "t18", "Advanced", "2020"), ("d26", "t24", "Advanced", "2021"), ("d26", "t09", "Proficient", "2019"),
        ("d27", "t03", "Advanced", "2019"), ("d27", "t20", "Advanced", "2020"), ("d27", "t25", "Proficient", "2021"), ("d27", "t22", "Proficient", "2022"),
        ("d28", "t05", "Expert", "2021"), ("d28", "t06", "Advanced", "2022"), ("d28", "t07", "Proficient", "2022"),
    };

    foreach (var (developerId, technologyId, proficiency, since) in data)
    {
        db.HasSkillRelations.Add(new HasSkillEdge { DeveloperId = developerId, TechnologyId = technologyId, Proficiency = proficiency, Since = since });
    }
}

static void SeedContributedTo(AppDbContext db)
{
    var data = new[]
    {
        ("d1", "r01", 120, "2021"), ("d1", "r03", 35, "2021"),
        ("d2", "r02", 80, "2022"),
        ("d3", "r08", 60, "2023"), ("d3", "r09", 50, "2023"),
        ("d4", "r03", 45, "2021"),
        ("d5", "r04", 90, "2022"),
        ("d6", "r06", 70, "2022"),
        ("d7", "r02", 20, "2021"), ("d7", "r08", 15, "2023"),
        ("d8", "r11", 60, "2023"),
        ("d9", "r05", 55, "2022"), ("d9", "r13", 40, "2024"),
        ("d10", "r09", 45, "2023"), ("d10", "r14", 30, "2024"),
        ("d11", "r10", 65, "2022"),
        ("d12", "r12", 85, "2020"),
        ("d13", "r07", 50, "2022"), ("d13", "r14", 40, "2024"),
        ("d14", "r01", 25, "2021"),
        ("d15", "r10", 40, "2022"),
        ("d16", "r10", 18, "2022"),
        ("d17", "r01", 60, "2022"), ("d17", "r16", 45, "2023"),
        ("d18", "r15", 90, "2023"), ("d18", "r12", 30, "2020"),
        ("d19", "r17", 55, "2024"), ("d19", "r14", 35, "2024"),
        ("d20", "r17", 40, "2024"), ("d20", "r13", 25, "2024"),
        ("d21", "r15", 50, "2023"), ("d21", "r04", 25, "2023"),
        ("d22", "r18", 45, "2023"), ("d22", "r16", 30, "2023"),
        ("d23", "r18", 20, "2023"), ("d23", "r20", 15, "2024"),
        ("d24", "r19", 65, "2022"), ("d24", "r01", 25, "2022"),
        ("d25", "r18", 35, "2023"), ("d25", "r10", 25, "2023"),
        ("d26", "r20", 70, "2024"), ("d26", "r18", 30, "2023"),
        ("d27", "r15", 55, "2023"), ("d27", "r19", 25, "2024"),
        ("d28", "r16", 45, "2023"), ("d28", "r21", 20, "2024"),
    };

    foreach (var (developerId, repositoryId, count, since) in data)
    {
        db.ContributedToRelations.Add(new ContributedToEdge { DeveloperId = developerId, RepositoryId = repositoryId, ContributionCount = count, Since = since });
    }
}

static void SeedRequiresSkill(AppDbContext db)
{
    var data = new[]
    {
        ("sk1", "t01"), ("sk1", "t09"),
        ("sk2", "t01"), ("sk2", "t05"),
        ("sk3", "t09"),
        ("sk4", "t01"), ("sk4", "t05"),
        ("sk5", "t03"), ("sk5", "t05"),
        ("sk6", "t05"), ("sk6", "t06"),
        ("sk7", "t19"), ("sk7", "t20"),
        ("sk8", "t03"), ("sk8", "t09"),
        ("sk9", "t08"), ("sk9", "t10"),
        ("sk10", "t08"), ("sk10", "t12"),
        ("sk11", "t05"), ("sk11", "t06"),
        ("sk12", "t01"), ("sk12", "t06"),
        ("sk13", "t01"), ("sk13", "t09"),
        ("sk14", "t05"), ("sk14", "t06"),
        ("sk15", "t02"), ("sk15", "t04"),
        ("sk16", "t04"), ("sk16", "t16"),
        ("sk17", "t05"),
        ("sk18", "t08"), ("sk18", "t11"),
        ("sk19", "t12"),
        ("sk20", "t11"), ("sk20", "t12"),
        ("sk21", "t08"),
        ("sk22", "t03"), ("sk22", "t19"),
        ("sk23", "t03"), ("sk23", "t20"),
        ("sk24", "t19"), ("sk24", "t15"),
        ("sk25", "t08"), ("sk25", "t18"),
        ("sk26", "t11"), ("sk26", "t05"),
        ("sk27", "t08"), ("sk27", "t18"),
        ("sk28", "t12"), ("sk28", "t04"),
        ("sk29", "t04"), ("sk29", "t03"),
        ("sk30", "t05"), ("sk30", "t11"),
        ("sk31", "t03"), ("sk31", "t17"),
        ("sk32", "t20"), ("sk32", "t22"),
        ("sk33", "t20"), ("sk33", "t25"),
        ("sk34", "t03"), ("sk34", "t25"),
        ("sk35", "t01"), ("sk35", "t13"),
        ("sk36", "t13"), ("sk36", "t14"),
        ("sk37", "t05"), ("sk37", "t16"),
        ("sk38", "t08"), ("sk38", "t23"),
        ("sk39", "t06"), ("sk39", "t05"),
        ("sk40", "t01"), ("sk40", "t18"),
        ("sk41", "t01"), ("sk41", "t12"),
        ("sk42", "t05"), ("sk42", "t18"),
        ("sk43", "t02"), ("sk43", "t21"),
        ("sk44", "t02"), ("sk44", "t18"),
        ("sk45", "t26"), ("sk45", "t13"),
        ("sk46", "t26"), ("sk46", "t24"),
    };

    foreach (var (taskId, technologyId) in data)
    {
        db.RequiresSkillRelations.Add(new RequiresSkillEdge { TaskId = taskId, TechnologyId = technologyId });
    }
}