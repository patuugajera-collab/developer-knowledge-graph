# Developer Knowledge Graph

A full-stack application for exploring a developer knowledge and dependency graph. It models developers, projects, technologies, repositories, tasks, and organizations as graph nodes connected by typed relationships (e.g. `WORKS_ON`, `USES`, `DEPENDS_ON`, `HAS_SKILL`), and exposes that graph through a REST API and an interactive Angular frontend with a force-directed graph explorer.

## Stack

| Layer     | Technology                                                                  |
|-----------|-----------------------------------------------------------------------------|
| Backend   | ASP.NET Core (.NET 10), EF Core (SQL Server), Swagger/OpenAPI                |
| Frontend  | Angular 19, Angular Material, D3-force (graph rendering)                     |
| Database  | SQL Server (LocalDB by default)                                              |

## Repository layout

```
├── database/cypher/          Cypher reference scripts (original graph queries)
├── src/
│   ├── Api/                  ASP.NET Core REST API
│   └── SeedTool/             Console app that creates & seeds the database
├── frontend/developer-knowledge-graph/   Angular SPA
└── tests/                    xUnit integration & unit tests
```

Behaviorally, path-based queries (dependency chains, neighbourhood graphs, shortest paths) run as in-memory breadth-first traversals over the edge tables, preserving the semantics of the original Cypher queries on this small graph.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 18+ and npm
- [Angular CLI](https://angular.dev/cli) 19 (`npm install -g @angular/cli`)
- SQL Server (LocalDB is used by default on Windows)

## Getting started

### 1. Database

Seed the database (creates the schema and inserts sample data):

```powershell
dotnet run --project src/SeedTool
```

This uses the connection string `Server=(localdb)\MSSQLLocalDB;Database=DeveloperKnowledgeGraph;Trusted_Connection=True;TrustServerCertificate=True;` by default. To use a different connection string, pass it as an argument or set the `DEFAULT_CONNECTION` environment variable.

### 2. API configuration

Copy `.env.example` to `.env` and set your CORS origins:

```powershell
Copy-Item .env.example .env
```

`.env` is gitignored and must never be committed. Set `CORS_ALLOWED_ORIGINS` to your Angular dev server and any deployed frontend (default: `http://localhost:4200`).

You can also override the connection string via `appsettings.json` (`ConnectionStrings:DefaultConnection`).

### 3. Run the API

```powershell
dotnet run --project src/Api
```

The API listens on `http://localhost:61248` (see `Properties/launchSettings.json`) and exposes Swagger UI at `/swagger` in development.

> The frontend proxies `/api` calls to `http://localhost:61248` (see `proxy.conf.json`). Update the target if your API runs on a different port.

### 4. Run the frontend

```bash
cd frontend/developer-knowledge-graph
npm install
npm start        # ng serve → http://localhost:4200
```

Open `http://localhost:4200` and browse developers, projects, technologies, and organizations, or explore the full graph with the graph explorer.

## API endpoints

Key endpoints exposed by the API:

- `GET /api/health` – service health
- `GET /api/developers` – list/search developers (paged)
- `GET /api/developers/{id}` – developer detail, projects, skills, repositories
- `GET /api/projects` – list/search projects (paged, filterable by status)
- `GET /api/projects/{id}` – project detail, dependencies, technologies, developers, contributors, tasks, recommended developers, indirect technologies
- `GET /api/technologies` – list/search technologies (paged, filterable by category)
- `GET /api/technologies/{id}` – technology detail, developers, projects
- `GET /api/organizations` – organizations with developer/project counts
- `GET /api/search` – unified search across all node types
- `GET /api/dashboard` – aggregate counts (nodes, relationships, etc.)
- `GET /api/graph/{id}` – neighbourhood graph up to `maxDepth`
- `GET /api/graph/shortest-path` – shortest path between a developer and a project

## Tests

```powershell
dotnet test
```

Runs the xUnit suite in `tests/DeveloperKnowledgeGraph.Tests`, covering the service layer, the graph depth guard, exception handling middleware, pagination, and API integration.

## Reference Cypher scripts

The original graph queries used to drive the design live in [`database/cypher/`](database/cypher/):

| File | Purpose |
|------|---------|
| `01_create_constraints.cypher` | Indexes and constraints |
| `02_seed_data.cypher` | Sample graph data |
| `03_developers_by_technology.cypher` | Find developers by technology |
| `04_project_dependencies.cypher` | Traverse dependency chains |
| `05_developer_project_connections.cypher` | Developer↔project links |
| `06_skill_matching.cypher` | Match candidates to required skills |
| `07_graph_path.cypher` | Shortest-path exploration |
| `08_project_overview.cypher` | Project summary queries |
