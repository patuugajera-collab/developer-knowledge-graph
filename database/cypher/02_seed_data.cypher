// Developer Knowledge & Dependency Graph - seed data.
// Executable against CognoDB (openCypher over Bolt). Fully idempotent:
// re-running this file only updates properties and never duplicates nodes or relationships.

// ---------------------------------------------------------------------------
// Organizations
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'o1', name: 'Acme Corp'},
  {id: 'o2', name: 'Nimbus Labs'},
  {id: 'o3', name: 'Helix Industries'},
  {id: 'o4', name: 'Beacon Soft'},
  {id: 'o5', name: 'OpenVox'}
] AS org
MERGE (o:Organization {id: org.id})
SET o.name = org.name;

// ---------------------------------------------------------------------------
// Developers
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'd1',  name: 'Alice Chen',      email: 'alice.chen@example.com',    role: 'Backend Engineer'},
  {id: 'd2',  name: 'Marcus Johnson',  email: 'marcus.johnson@example.com', role: 'Frontend Engineer'},
  {id: 'd3',  name: 'Priya Sharma',    email: 'priya.sharma@example.com',  role: 'Full-Stack Engineer'},
  {id: 'd4',  name: 'Diego Martinez',  email: 'diego.martinez@example.com', role: 'DevOps Engineer'},
  {id: 'd5',  name: 'Emily Watson',    email: 'emily.watson@example.com',  role: 'Data Engineer'},
  {id: 'd6',  name: 'Omar Farouk',     email: 'omar.farouk@example.com',   role: 'ML Engineer'},
  {id: 'd7',  name: 'Sofia Rossi',     email: 'sofia.rossi@example.com',   role: 'QA Engineer'},
  {id: 'd8',  name: "Liam O'Brien",    email: 'liam.obrien@example.com',   role: 'Backend Engineer'},
  {id: 'd9',  name: 'Yuki Tanaka',     email: 'yuki.tanaka@example.com',   role: 'Frontend Engineer'},
  {id: 'd10', name: 'Aisha Bello',     email: 'aisha.bello@example.com',   role: 'Product Engineer'},
  {id: 'd11', name: 'Noah Kim',        email: 'noah.kim@example.com',      role: 'DevOps Engineer'},
  {id: 'd12', name: 'Freya Novak',     email: 'freya.novak@example.com',   role: 'Data Engineer'},
  {id: 'd13', name: 'Kenji Sato',      email: 'kenji.sato@example.com',    role: 'Mobile Engineer'},
  {id: 'd14', name: 'Lucia Blanco',    email: 'lucia.blanco@example.com',  role: 'Engineering Manager'},
  {id: 'd15', name: 'Tariq Haddad',    email: 'tariq.haddad@example.com',  role: 'Security Engineer'},
  {id: 'd16', name: 'Emma Larsson',    email: 'emma.larsson@example.com',  role: 'QA Engineer'}
] AS dev
MERGE (d:Developer {id: dev.id})
SET d.name = dev.name, d.email = dev.email, d.role = dev.role;

// ---------------------------------------------------------------------------
// Projects
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'p1', name: 'Atlas ERP',                  description: 'Enterprise resource planning suite for mid-size organisations.', status: 'active'},
  {id: 'p2', name: 'Nova Analytics Platform',    description: 'Self-service BI and analytics over streaming and batch data.',       status: 'active'},
  {id: 'p3', name: 'Pulse Health Monitor',       description: 'Real-time patient vitals monitoring with alerting.',                status: 'active'},
  {id: 'p4', name: 'Quantum Commerce',           description: 'High-volume e-commerce storefront and checkout platform.',         status: 'in_progress'},
  {id: 'p5', name: 'Sentinel Security Suite',    description: 'Vulnerability scanning and threat intelligence platform.',          status: 'active'},
  {id: 'p6', name: 'Orbit Task Scheduler',       description: 'Distributed, dependency-aware job scheduler.',                     status: 'in_progress'},
  {id: 'p7', name: 'Flux Data Pipeline',         description: 'Replayable stream ingestion and enrichment pipeline.',             status: 'maintenance'},
  {id: 'p8', name: 'Helios Chat Platform',       description: 'Real-time chat and presence platform with bot integrations.',      status: 'planning'},
  {id: 'p9', name: 'Zenith Mobile Wallet',       description: 'Digital wallet with P2P transfers and fraud detection.',          status: 'in_progress'}
] AS project
MERGE (p:Project {id: project.id})
SET p.name = project.name, p.description = project.description, p.status = project.status;

// ---------------------------------------------------------------------------
// Technologies
// ---------------------------------------------------------------------------
UNWIND [
  {id: 't01', name: '.NET / C#',  category: 'Backend'},
  {id: 't02', name: 'Java',       category: 'Backend'},
  {id: 't03', name: 'Python',     category: 'Backend'},
  {id: 't04', name: 'Go',         category: 'Backend'},
  {id: 't05', name: 'TypeScript', category: 'Frontend'},
  {id: 't06', name: 'React',      category: 'Frontend'},
  {id: 't07', name: 'Angular',    category: 'Frontend'},
  {id: 't08', name: 'Node.js',    category: 'Backend'},
  {id: 't09', name: 'PostgreSQL', category: 'Database'},
  {id: 't10', name: 'MongoDB',    category: 'Database'},
  {id: 't11', name: 'Neo4j',      category: 'Database'},
  {id: 't12', name: 'Redis',      category: 'Infrastructure'},
  {id: 't13', name: 'Docker',     category: 'DevOps'},
  {id: 't14', name: 'Kubernetes', category: 'DevOps'},
  {id: 't15', name: 'AWS',        category: 'Cloud'},
  {id: 't16', name: 'Azure',      category: 'Cloud'},
  {id: 't17', name: 'TensorFlow', category: 'ML'},
  {id: 't18', name: 'GraphQL',    category: 'API'},
  {id: 't19', name: 'Apache Kafka', category: 'Streaming'},
  {id: 't20', name: 'Apache Spark', category: 'Big Data'}
] AS tech
MERGE (t:Technology {id: tech.id})
SET t.name = tech.name, t.category = tech.category;

// ---------------------------------------------------------------------------
// Repositories
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'r01', name: 'atlas-api',          url: 'https://github.com/acme/atlas-api',          project: 'p1'},
  {id: 'r02', name: 'atlas-web',          url: 'https://github.com/acme/atlas-web',          project: 'p1'},
  {id: 'r03', name: 'atlas-infra',        url: 'https://github.com/acme/atlas-infra',        project: 'p1'},
  {id: 'r04', name: 'nova-query-engine',  url: 'https://github.com/acme/nova-query-engine',  project: 'p2'},
  {id: 'r05', name: 'nova-dashboard',     url: 'https://github.com/acme/nova-dashboard',     project: 'p2'},
  {id: 'r06', name: 'pulse-backend',      url: 'https://github.com/beacon/pulse-backend',    project: 'p3'},
  {id: 'r07', name: 'pulse-mobile',       url: 'https://github.com/beacon/pulse-mobile',     project: 'p3'},
  {id: 'r08', name: 'quantum-storefront', url: 'https://github.com/nimbus/quantum-storefront', project: 'p4'},
  {id: 'r09', name: 'quantum-checkout',   url: 'https://github.com/nimbus/quantum-checkout', project: 'p4'},
  {id: 'r10', name: 'sentinel-scanner',   url: 'https://github.com/openvox/sentinel-scanner', project: 'p5'},
  {id: 'r11', name: 'orbit-scheduler',    url: 'https://github.com/helix/orbit-scheduler',   project: 'p6'},
  {id: 'r12', name: 'flux-pipeline',      url: 'https://github.com/acme/flux-pipeline',      project: 'p7'},
  {id: 'r13', name: 'helios-gateway',     url: 'https://github.com/beacon/helios-gateway',   project: 'p8'},
  {id: 'r14', name: 'zenith-wallet-app',  url: 'https://github.com/nimbus/zenith-wallet-app', project: 'p9'}
] AS repo
MERGE (r:Repository {id: repo.id})
SET r.name = repo.name, r.url = repo.url
WITH r, repo
MATCH (p:Project {id: repo.project})
MERGE (r)-[:BELONGS_TO]->(p);

// ---------------------------------------------------------------------------
// Tasks
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'sk1',  title: 'Build inventory ledger API',          status: 'done',        priority: 1, project: 'p1'},
  {id: 'sk2',  title: 'Implement OAuth2 SSO',                status: 'in_progress', priority: 2, project: 'p1'},
  {id: 'sk3',  title: 'Migrate customer records',            status: 'todo',        priority: 3, project: 'p1'},
  {id: 'sk4',  title: 'Add reporting endpoints',             status: 'backlog',     priority: 4, project: 'p1'},
  {id: 'sk5',  title: 'Design query engine DSL',             status: 'done',        priority: 1, project: 'p2'},
  {id: 'sk6',  title: 'Build dashboard widgets',             status: 'in_progress', priority: 2, project: 'p2'},
  {id: 'sk7',  title: 'Connect streaming ingestion',         status: 'todo',        priority: 2, project: 'p2'},
  {id: 'sk8',  title: 'Alerting rules engine',               status: 'backlog',     priority: 3, project: 'p2'},
  {id: 'sk9',  title: 'Implement vitals ingestion',          status: 'done',        priority: 1, project: 'p3'},
  {id: 'sk10', title: 'Build alerting pipeline',             status: 'in_progress', priority: 2, project: 'p3'},
  {id: 'sk11', title: 'Mobile push notifications',           status: 'todo',        priority: 3, project: 'p3'},
  {id: 'sk12', title: 'Cart checkout flow',                  status: 'in_progress', priority: 1, project: 'p4'},
  {id: 'sk13', title: 'Payment gateway integration',         status: 'todo',        priority: 1, project: 'p4'},
  {id: 'sk14', title: 'Product catalog search',              status: 'done',        priority: 2, project: 'p4'},
  {id: 'sk15', title: 'Build vulnerability scanner',         status: 'done',        priority: 1, project: 'p5'},
  {id: 'sk16', title: 'Connect threat intel feed',           status: 'in_progress', priority: 2, project: 'p5'},
  {id: 'sk17', title: 'Generate compliance reports',         status: 'todo',        priority: 3, project: 'p5'},
  {id: 'sk18', title: 'Scheduler core loop',                 status: 'in_progress', priority: 1, project: 'p6'},
  {id: 'sk19', title: 'Retry policy engine',                 status: 'todo',        priority: 2, project: 'p6'},
  {id: 'sk20', title: 'Service integration hooks',           status: 'backlog',     priority: 3, project: 'p6'},
  {id: 'sk21', title: 'Audit log exporter',                  status: 'backlog',     priority: 4, project: 'p6'},
  {id: 'sk22', title: 'Replayable pipeline stages',          status: 'done',        priority: 1, project: 'p7'},
  {id: 'sk23', title: 'Schema registry',                     status: 'in_progress', priority: 2, project: 'p7'},
  {id: 'sk24', title: 'Dead letter queue',                   status: 'todo',        priority: 3, project: 'p7'},
  {id: 'sk25', title: 'Chat message routing',                status: 'todo',        priority: 1, project: 'p8'},
  {id: 'sk26', title: 'Real-time presence',                  status: 'backlog',     priority: 2, project: 'p8'},
  {id: 'sk27', title: 'Bot framework integration',           status: 'backlog',     priority: 3, project: 'p8'},
  {id: 'sk28', title: 'Wallet balance cache',                status: 'in_progress', priority: 1, project: 'p9'},
  {id: 'sk29', title: 'P2P transfers',                       status: 'todo',        priority: 1, project: 'p9'},
  {id: 'sk30', title: 'KYC verification',                    status: 'todo',        priority: 2, project: 'p9'},
  {id: 'sk31', title: 'Fraud detection rules',               status: 'backlog',     priority: 3, project: 'p9'}
] AS task
MERGE (t:Task {id: task.id})
SET t.title = task.title, t.status = task.status, t.priority = task.priority
WITH t, task
MATCH (p:Project {id: task.project})
MERGE (p)-[:HAS_TASK]->(t);

// ---------------------------------------------------------------------------
// WORKS_FOR: Developer -> Organization
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'd1', org: 'o1'}, {id: 'd2', org: 'o1'}, {id: 'd5', org: 'o1'},
  {id: 'd9', org: 'o1'}, {id: 'd14', org: 'o1'},
  {id: 'd3', org: 'o2'}, {id: 'd10', org: 'o2'}, {id: 'd13', org: 'o2'},
  {id: 'd4', org: 'o3'}, {id: 'd8', org: 'o3'}, {id: 'd11', org: 'o3'},
  {id: 'd6', org: 'o4'}, {id: 'd12', org: 'o4'}, {id: 'd16', org: 'o4'},
  {id: 'd7', org: 'o5'}, {id: 'd15', org: 'o5'}
] AS rel
MATCH (d:Developer {id: rel.id})
MATCH (o:Organization {id: rel.org})
MERGE (d)-[r:WORKS_FOR {since: '2020'}]->(o);

// ---------------------------------------------------------------------------
// OWNS: Organization -> Project
// ---------------------------------------------------------------------------
UNWIND [
  {org: 'o1', project: 'p1'}, {org: 'o1', project: 'p2'}, {org: 'o1', project: 'p7'},
  {org: 'o2', project: 'p4'}, {org: 'o2', project: 'p9'},
  {org: 'o3', project: 'p6'},
  {org: 'o4', project: 'p3'}, {org: 'o4', project: 'p8'},
  {org: 'o5', project: 'p5'}
] AS rel
MATCH (o:Organization {id: rel.org})
MATCH (p:Project {id: rel.project})
MERGE (o)-[:OWNS]->(p);

// ---------------------------------------------------------------------------
// WORKS_ON: Developer -> Project
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'd1', project: 'p1', role: 'Lead',        since: '2021'},
  {id: 'd1', project: 'p6', role: 'Contributor', since: '2023'},
  {id: 'd2', project: 'p2', role: 'Lead',        since: '2022'},
  {id: 'd2', project: 'p8', role: 'Contributor', since: '2024'},
  {id: 'd3', project: 'p4', role: 'Lead',        since: '2023'},
  {id: 'd3', project: 'p1', role: 'Contributor', since: '2021'},
  {id: 'd4', project: 'p1', role: 'Infra',       since: '2021'},
  {id: 'd4', project: 'p5', role: 'Member',      since: '2022'},
  {id: 'd5', project: 'p2', role: 'Lead',        since: '2022'},
  {id: 'd5', project: 'p7', role: 'Contributor', since: '2020'},
  {id: 'd6', project: 'p3', role: 'Lead',        since: '2022'},
  {id: 'd6', project: 'p8', role: 'Contributor', since: '2024'},
  {id: 'd7', project: 'p1', role: 'QA',          since: '2021'},
  {id: 'd7', project: 'p4', role: 'QA',          since: '2023'},
  {id: 'd8', project: 'p6', role: 'Lead',        since: '2023'},
  {id: 'd8', project: 'p5', role: 'Contributor', since: '2022'},
  {id: 'd9', project: 'p8', role: 'Lead',        since: '2024'},
  {id: 'd9', project: 'p2', role: 'Contributor', since: '2022'},
  {id: 'd10', project: 'p4', role: 'Product',     since: '2023'},
  {id: 'd10', project: 'p9', role: 'Lead',        since: '2024'},
  {id: 'd11', project: 'p5', role: 'DevOps',     since: '2022'},
  {id: 'd11', project: 'p2', role: 'Contributor', since: '2023'},
  {id: 'd12', project: 'p7', role: 'Lead',       since: '2020'},
  {id: 'd12', project: 'p9', role: 'Contributor', since: '2024'},
  {id: 'd13', project: 'p9', role: 'Mobile',     since: '2024'},
  {id: 'd13', project: 'p3', role: 'Contributor', since: '2022'},
  {id: 'd14', project: 'p1', role: 'Manager',    since: '2021'},
  {id: 'd14', project: 'p3', role: 'Manager',    since: '2022'},
  {id: 'd15', project: 'p5', role: 'Security',   since: '2022'},
  {id: 'd15', project: 'p1', role: 'Consultant', since: '2021'},
  {id: 'd16', project: 'p5', role: 'QA',         since: '2022'},
  {id: 'd16', project: 'p9', role: 'QA',         since: '2024'}
] AS rel
MATCH (d:Developer {id: rel.id})
MATCH (p:Project {id: rel.project})
MERGE (d)-[r:WORKS_ON]->(p)
SET r.role = rel.role, r.since = rel.since;

// ---------------------------------------------------------------------------
// USES: Project -> Technology
// ---------------------------------------------------------------------------
UNWIND [
  {project: 'p1', tech: 't01', purpose: 'build'},
  {project: 'p1', tech: 't05', purpose: 'frontend'},
  {project: 'p1', tech: 't09', purpose: 'data'},
  {project: 'p1', tech: 't13', purpose: 'deployment'},
  {project: 'p2', tech: 't03', purpose: 'processing'},
  {project: 'p2', tech: 't19', purpose: 'streaming'},
  {project: 'p2', tech: 't20', purpose: 'batch'},
  {project: 'p2', tech: 't05', purpose: 'frontend'},
  {project: 'p3', tech: 't08', purpose: 'runtime'},
  {project: 'p3', tech: 't10', purpose: 'storage'},
  {project: 'p3', tech: 't12', purpose: 'caching'},
  {project: 'p3', tech: 't17', purpose: 'prediction'},
  {project: 'p4', tech: 't01', purpose: 'build'},
  {project: 'p4', tech: 't06', purpose: 'frontend'},
  {project: 'p4', tech: 't09', purpose: 'data'},
  {project: 'p4', tech: 't12', purpose: 'caching'},
  {project: 'p5', tech: 't02', purpose: 'build'},
  {project: 'p5', tech: 't04', purpose: 'scanner'},
  {project: 'p5', tech: 't16', purpose: 'cloud'},
  {project: 'p6', tech: 't08', purpose: 'runtime'},
  {project: 'p6', tech: 't11', purpose: 'dependencies'},
  {project: 'p6', tech: 't12', purpose: 'queueing'},
  {project: 'p7', tech: 't03', purpose: 'processing'},
  {project: 'p7', tech: 't19', purpose: 'streaming'},
  {project: 'p7', tech: 't20', purpose: 'batch'},
  {project: 'p7', tech: 't15', purpose: 'cloud'},
  {project: 'p8', tech: 't05', purpose: 'build'},
  {project: 'p8', tech: 't08', purpose: 'runtime'},
  {project: 'p8', tech: 't11', purpose: 'presence'},
  {project: 'p8', tech: 't18', purpose: 'api'},
  {project: 'p9', tech: 't04', purpose: 'api'},
  {project: 'p9', tech: 't03', purpose: 'services'},
  {project: 'p9', tech: 't14', purpose: 'infrastructure'},
  {project: 'p9', tech: 't15', purpose: 'cloud'}
] AS rel
MATCH (p:Project {id: rel.project})
MATCH (t:Technology {id: rel.tech})
MERGE (p)-[r:USES]->(t)
SET r.purpose = rel.purpose;

// ---------------------------------------------------------------------------
// DEPENDS_ON: Project -> Project (dependency graph)
// ---------------------------------------------------------------------------
UNWIND [
  {project: 'p2', on: 'p7'}, {project: 'p2', on: 'p1'},
  {project: 'p3', on: 'p1'},
  {project: 'p4', on: 'p1'},
  {project: 'p5', on: 'p1'},
  {project: 'p6', on: 'p1'},
  {project: 'p7', on: 'p1'},
  {project: 'p8', on: 'p6'},
  {project: 'p9', on: 'p4'}, {project: 'p9', on: 'p8'}
] AS rel
MATCH (p:Project {id: rel.project})
MATCH (dep:Project {id: rel.on})
MERGE (p)-[:DEPENDS_ON]->(dep);

// ---------------------------------------------------------------------------
// HAS_SKILL: Developer -> Technology
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'd1', tech: 't01', proficiency: 'Expert',       since: '2018'},
  {id: 'd1', tech: 't09', proficiency: 'Advanced',     since: '2019'},
  {id: 'd1', tech: 't05', proficiency: 'Proficient',   since: '2020'},
  {id: 'd1', tech: 't11', proficiency: 'Advanced',     since: '2021'},
  {id: 'd2', tech: 't05', proficiency: 'Expert',       since: '2019'},
  {id: 'd2', tech: 't06', proficiency: 'Advanced',     since: '2020'},
  {id: 'd2', tech: 't07', proficiency: 'Advanced',     since: '2019'},
  {id: 'd2', tech: 't18', proficiency: 'Proficient',   since: '2021'},
  {id: 'd3', tech: 't03', proficiency: 'Advanced',     since: '2018'},
  {id: 'd3', tech: 't05', proficiency: 'Advanced',     since: '2019'},
  {id: 'd3', tech: 't01', proficiency: 'Proficient',   since: '2020'},
  {id: 'd3', tech: 't09', proficiency: 'Proficient',   since: '2020'},
  {id: 'd4', tech: 't13', proficiency: 'Advanced',     since: '2019'},
  {id: 'd4', tech: 't14', proficiency: 'Expert',       since: '2020'},
  {id: 'd4', tech: 't15', proficiency: 'Proficient',   since: '2021'},
  {id: 'd5', tech: 't03', proficiency: 'Expert',       since: '2018'},
  {id: 'd5', tech: 't20', proficiency: 'Advanced',     since: '2019'},
  {id: 'd5', tech: 't19', proficiency: 'Proficient',   since: '2021'},
  {id: 'd5', tech: 't09', proficiency: 'Advanced',     since: '2018'},
  {id: 'd6', tech: 't03', proficiency: 'Advanced',     since: '2018'},
  {id: 'd6', tech: 't17', proficiency: 'Expert',       since: '2019'},
  {id: 'd6', tech: 't20', proficiency: 'Proficient',   since: '2020'},
  {id: 'd7', tech: 't05', proficiency: 'Proficient',   since: '2020'},
  {id: 'd7', tech: 't06', proficiency: 'Advanced',     since: '2021'},
  {id: 'd8', tech: 't01', proficiency: 'Advanced',     since: '2018'},
  {id: 'd8', tech: 't08', proficiency: 'Proficient',   since: '2020'},
  {id: 'd8', tech: 't09', proficiency: 'Advanced',     since: '2019'},
  {id: 'd8', tech: 't11', proficiency: 'Proficient',   since: '2021'},
  {id: 'd9', tech: 't05', proficiency: 'Expert',       since: '2018'},
  {id: 'd9', tech: 't07', proficiency: 'Expert',       since: '2019'},
  {id: 'd9', tech: 't06', proficiency: 'Advanced',     since: '2020'},
  {id: 'd9', tech: 't18', proficiency: 'Advanced',     since: '2021'},
  {id: 'd10', tech: 't03', proficiency: 'Proficient', since: '2020'},
  {id: 'd10', tech: 't05', proficiency: 'Expert',     since: '2018'},
  {id: 'd10', tech: 't11', proficiency: 'Proficient', since: '2021'},
  {id: 'd10', tech: 't18', proficiency: 'Advanced',   since: '2019'},
  {id: 'd11', tech: 't14', proficiency: 'Advanced',   since: '2020'},
  {id: 'd11', tech: 't13', proficiency: 'Advanced',   since: '2021'},
  {id: 'd11', tech: 't15', proficiency: 'Expert',     since: '2019'},
  {id: 'd11', tech: 't04', proficiency: 'Proficient', since: '2022'},
  {id: 'd12', tech: 't03', proficiency: 'Expert',     since: '2017'},
  {id: 'd12', tech: 't20', proficiency: 'Advanced',   since: '2019'},
  {id: 'd12', tech: 't09', proficiency: 'Proficient', since: '2020'},
  {id: 'd12', tech: 't19', proficiency: 'Advanced',   since: '2020'},
  {id: 'd13', tech: 't06', proficiency: 'Proficient', since: '2021'},
  {id: 'd13', tech: 't08', proficiency: 'Proficient', since: '2019'},
  {id: 'd13', tech: 't05', proficiency: 'Advanced',   since: '2020'},
  {id: 'd13', tech: 't10', proficiency: 'Advanced',   since: '2019'},
  {id: 'd14', tech: 't01', proficiency: 'Proficient', since: '2017'},
  {id: 'd14', tech: 't05', proficiency: 'Advanced',   since: '2019'},
  {id: 'd14', tech: 't11', proficiency: 'Proficient', since: '2021'},
  {id: 'd15', tech: 't02', proficiency: 'Advanced',   since: '2018'},
  {id: 'd15', tech: 't04', proficiency: 'Advanced',   since: '2019'},
  {id: 'd15', tech: 't16', proficiency: 'Proficient', since: '2020'},
  {id: 'd16', tech: 't05', proficiency: 'Proficient', since: '2020'},
  {id: 'd16', tech: 't06', proficiency: 'Proficient', since: '2021'},
  {id: 'd16', tech: 't18', proficiency: 'Advanced',   since: '2021'}
] AS rel
MATCH (d:Developer {id: rel.id})
MATCH (t:Technology {id: rel.tech})
MERGE (d)-[r:HAS_SKILL]->(t)
SET r.proficiency = rel.proficiency, r.since = rel.since;

// ---------------------------------------------------------------------------
// CONTRIBUTED_TO: Developer -> Repository
// ---------------------------------------------------------------------------
UNWIND [
  {id: 'd1', repo: 'r01', count: 120, since: '2021'},
  {id: 'd1', repo: 'r03', count: 35,  since: '2021'},
  {id: 'd2', repo: 'r02', count: 80,  since: '2022'},
  {id: 'd3', repo: 'r08', count: 60,  since: '2023'},
  {id: 'd3', repo: 'r09', count: 50,  since: '2023'},
  {id: 'd4', repo: 'r03', count: 45,  since: '2021'},
  {id: 'd5', repo: 'r04', count: 90,  since: '2022'},
  {id: 'd6', repo: 'r06', count: 70,  since: '2022'},
  {id: 'd7', repo: 'r02', count: 20,  since: '2021'},
  {id: 'd7', repo: 'r08', count: 15,  since: '2023'},
  {id: 'd8', repo: 'r11', count: 60,  since: '2023'},
  {id: 'd9', repo: 'r05', count: 55,  since: '2022'},
  {id: 'd9', repo: 'r13', count: 40,  since: '2024'},
  {id: 'd10', repo: 'r09', count: 45, since: '2023'},
  {id: 'd10', repo: 'r14', count: 30, since: '2024'},
  {id: 'd11', repo: 'r10', count: 65, since: '2022'},
  {id: 'd12', repo: 'r12', count: 85, since: '2020'},
  {id: 'd13', repo: 'r07', count: 50, since: '2022'},
  {id: 'd13', repo: 'r14', count: 40, since: '2024'},
  {id: 'd14', repo: 'r01', count: 25, since: '2021'},
  {id: 'd15', repo: 'r10', count: 40, since: '2022'},
  {id: 'd16', repo: 'r10', count: 18, since: '2022'}
] AS rel
MATCH (d:Developer {id: rel.id})
MATCH (r:Repository {id: rel.repo})
MERGE (d)-[c:CONTRIBUTED_TO]->(r)
SET c.contributionCount = rel.count, c.since = rel.since;

// ---------------------------------------------------------------------------
// REQUIRES_SKILL: Task -> Technology
// ---------------------------------------------------------------------------
UNWIND [
  {task: 'sk1',  tech: 't01'}, {task: 'sk1', tech: 't09'},
  {task: 'sk2',  tech: 't01'}, {task: 'sk2', tech: 't05'},
  {task: 'sk3',  tech: 't09'},
  {task: 'sk4',  tech: 't01'}, {task: 'sk4', tech: 't05'},
  {task: 'sk5',  tech: 't03'}, {task: 'sk5', tech: 't05'},
  {task: 'sk6',  tech: 't05'}, {task: 'sk6', tech: 't06'},
  {task: 'sk7',  tech: 't19'}, {task: 'sk7', tech: 't20'},
  {task: 'sk8',  tech: 't03'}, {task: 'sk8', tech: 't09'},
  {task: 'sk9',  tech: 't08'}, {task: 'sk9', tech: 't10'},
  {task: 'sk10', tech: 't08'}, {task: 'sk10', tech: 't12'},
  {task: 'sk11', tech: 't05'}, {task: 'sk11', tech: 't06'},
  {task: 'sk12', tech: 't01'}, {task: 'sk12', tech: 't06'},
  {task: 'sk13', tech: 't01'}, {task: 'sk13', tech: 't09'},
  {task: 'sk14', tech: 't05'}, {task: 'sk14', tech: 't06'},
  {task: 'sk15', tech: 't02'}, {task: 'sk15', tech: 't04'},
  {task: 'sk16', tech: 't04'}, {task: 'sk16', tech: 't16'},
  {task: 'sk17', tech: 't05'},
  {task: 'sk18', tech: 't08'}, {task: 'sk18', tech: 't11'},
  {task: 'sk19', tech: 't12'},
  {task: 'sk20', tech: 't11'}, {task: 'sk20', tech: 't12'},
  {task: 'sk21', tech: 't08'},
  {task: 'sk22', tech: 't03'}, {task: 'sk22', tech: 't19'},
  {task: 'sk23', tech: 't03'}, {task: 'sk23', tech: 't20'},
  {task: 'sk24', tech: 't19'}, {task: 'sk24', tech: 't15'},
  {task: 'sk25', tech: 't08'}, {task: 'sk25', tech: 't18'},
  {task: 'sk26', tech: 't11'}, {task: 'sk26', tech: 't05'},
  {task: 'sk27', tech: 't08'}, {task: 'sk27', tech: 't18'},
  {task: 'sk28', tech: 't12'}, {task: 'sk28', tech: 't04'},
  {task: 'sk29', tech: 't04'}, {task: 'sk29', tech: 't03'},
  {task: 'sk30', tech: 't05'}, {task: 'sk30', tech: 't11'},
  {task: 'sk31', tech: 't03'}, {task: 'sk31', tech: 't17'}
] AS rel
MATCH (t:Task {id: rel.task})
MATCH (tech:Technology {id: rel.tech})
MERGE (t)-[:REQUIRES_SKILL]->(tech);